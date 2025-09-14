using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces.Services;
using Cuppie.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ilogger = Serilog.ILogger;

namespace Cuppie.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService, Ilogger logger, IOptions<JwtOptions> jwtOptions) : ControllerBase
    {
        private const string JwtCookieName = "jwt";
        private const string RefreshTokenCookieName = "refreshToken";

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterModelDto registerDto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var ip = GetIp();
            if (string.IsNullOrWhiteSpace(ip))
            {
                logger.Warning("Не удалось определить IP-адрес при логине пользователя {Username}", registerDto.Username);
                return Problem("Не удалось определить IP-адрес", statusCode: 400);
            }

            var authRequest = new AuthRequestDto()
            {
                Password = registerDto.Password,
                Username = registerDto.Username,
                Email = registerDto.Email,
                Ip = ip
            };

            if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
                authRequest.OldRefreshToken = refreshToken;

            var result = await authService.RegisterAsync(authRequest);

            if (result.ErrorCode == ErrorCode.Conflict)
                return Conflict("Пользователь с таким именем уже существует");

            if (result.IsSuccess)
            {
                SetTokenCookies(result.Data.JwtToken, result.Data.RefreshToken);
                return Ok(result.Data);
            }

            return Problem(
                detail: result.ErrorMessage ?? "Неизвестная ошибка при регистрации",
                statusCode: 400,
                title: "Ошибка регистрации"
            );
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginModelDto loginDto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var ip = GetIp();
            if (string.IsNullOrWhiteSpace(ip))
            {
                logger.Warning("IP отсутствует при логине пользователя {Username}", loginDto.Username);
                return Problem("Не удалось определить IP-адрес", statusCode: 400);
            }

            var authRequest = new AuthRequestDto()
            {
                Password = loginDto.Password,
                Username = loginDto.Username,
                Ip = ip
            };

            if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
                authRequest.OldRefreshToken = refreshToken;

            var result = await authService.LoginAsync(authRequest);

            if (result.IsSuccess)
            {
                SetTokenCookies(result.Data.JwtToken, result.Data.RefreshToken);
                return Ok(result.Data);
            }

            if (result.ErrorCode == ErrorCode.Unauthorized)
                return Unauthorized("Неверный логин или пароль");

            return Problem(
                detail: result.ErrorMessage ?? "Ошибка при логине",
                statusCode: 400,
                title: "Ошибка логина"
            );
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh()
        {
            if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var oldRefreshToken))
            {
                var ip = GetIp();
                if (string.IsNullOrWhiteSpace(ip))
                {
                    logger.Warning("Не удалось определить IP-адрес при обновлении токена");
                    return Problem("Не удалось определить IP-адрес", statusCode: 400);
                }

                var result = await authService.RefreshAccessTokenAsync(oldRefreshToken, ip);

                if (result.IsSuccess)
                {
                    SetTokenCookies(result.Data.JwtToken, result.Data.RefreshToken);
                    return Ok(result.Data);
                }

                return result.ErrorCode switch
                {
                    ErrorCode.Unauthorized => Unauthorized(result.ErrorMessage),
                    ErrorCode.NotFound => NotFound(result.ErrorMessage),
                    _ => Problem(detail: result.ErrorMessage, statusCode: 400, title: "Ошибка обновления токена")
                };
            }

            return Unauthorized("Отсутствует refresh токен в cookies");
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            DeleteCookies();
            return Ok();
        }

        [HttpGet("me")]
        public ActionResult<SafeUserDataDto> GetUserData()
        {
            if (!Request.Cookies.TryGetValue(JwtCookieName, out var jwt))
                return Unauthorized();

            var result = authService.GetCurrentUserData(jwt);

            if (result.ErrorCode == ErrorCode.Unauthorized)
                return Unauthorized("Неверные или просроченные jwt токен");

            if (!result.IsSuccess)
                return Problem(
                    detail: result.ErrorMessage,
                    statusCode: 400,
                    title: "Ошибка получения пользователя"
                );

            return Ok(result.Data);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        private string? GetIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private void DeleteCookies()
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Path = "/" // обязательно, если при установке был путь
            };

            Response.Cookies.Delete(JwtCookieName, options);
            Response.Cookies.Delete(RefreshTokenCookieName, options);
        }


        private void SetTokenCookies(TokenData accessJwtToken, TokenData refreshToken)
        {
            Response.Cookies.Append(JwtCookieName, accessJwtToken.Token, GetTokenCookieOptions(accessJwtToken.ExpiresInMinutes));
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken.Token, GetTokenCookieOptions(refreshToken.ExpiresInMinutes));
        }

        private CookieOptions GetTokenCookieOptions(int expiresInMinutes)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            };
        }
    }
}
