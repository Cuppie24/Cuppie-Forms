using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces;
using Cuppie.Application.Interfaces.Services;
using Cuppie.Infrastructure.Options;
using Cuppie.Infrastructure.Services;
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
        public async Task<IActionResult> Register([FromBody] RegisterModelDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Ошибки валидации
            }

            try
            {
                var ip = GetIp();
                if (string.IsNullOrWhiteSpace(ip))
                {
                    logger.Warning("Не удалось определить IP-адрес при логине пользователя {Username}", registerDto.Username);
                    return BadRequest("Не удалось определить IP-адрес.");
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
                
                if (result.ErrorCode == ErrorCode.Conflict) return Conflict("Пользователь с таким именем уже существует");
                if (result.IsSuccess)
                {
                    SetTokenCookies(result.Data.JwtToken, result.Data.RefreshToken);
                    return Ok(result.Data);
                }
                return BadRequest("Произошла ошибка");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDto loginDto)
        {
            logger.Information("Поступил запрос на api/auth/login с ip: {ip}", HttpContext.Connection.RemoteIpAddress);
            if (!ModelState.IsValid)
            {
                logger.Warning("Некорректные данные для логина");
                return BadRequest(ModelState);
            }

            var ip = GetIp();

            if (string.IsNullOrWhiteSpace(ip))
            {
                logger.Warning("Не удалось определить IP-адрес при логине пользователя {Username}", loginDto.Username);
                return BadRequest("Не удалось определить IP-адрес.");
            }

            var authRequest = new AuthRequestDto()
            {
                Password = loginDto.Password,
                Username = loginDto.Username,
                Ip = ip
            };
            if(Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
                authRequest.OldRefreshToken = refreshToken;
            
            
            var result = await authService.LoginAsync(authRequest);

            if (result.IsSuccess)
            {
                try
                {
                    SetTokenCookies(result.Data.JwtToken, result.Data.RefreshToken);
                    logger.Information("Логин прошел успешно, jwt и refresh куки отправлены");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Ошибка при прикреплении jwt куки к ответу");
                    return StatusCode(500, "Ошибка при создании jwt куки");
                }

                return Ok(result.Data);
            }

            if (result.ErrorCode is ErrorCode.Unauthorized)
            {
                logger.Information("Неверный логин или пароль");
                return Unauthorized("Неверный логин или пароль");
            }
            return BadRequest($"Прозошла ошибка: {result.ErrorMessage}");
        }
        
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var oldRefreshToken))
            {
                
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrWhiteSpace(ip))
                {
                    logger.Warning("Не удалось определить IP-адрес при обновлении токена");
                    return BadRequest("Не удалось определить IP-адрес.");
                }
                
                var authResponseDto = await authService.RefreshAccessTokenAsync(oldRefreshToken, ip);

                if (authResponseDto.IsSuccess)
                {

                    var newJwtToken = authResponseDto.Data.JwtToken;
                    var newRefreshToken = authResponseDto.Data.RefreshToken;
                    SetTokenCookies(newJwtToken, newRefreshToken);
                }

                return authResponseDto.ErrorCode switch
                {
                    ErrorCode.Unauthorized => Unauthorized(authResponseDto.ErrorMessage),
                    ErrorCode.NotFound => NotFound(authResponseDto.ErrorMessage),
                    _ => BadRequest(authResponseDto.ErrorMessage)
                };
            }
            return Unauthorized();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            DeleteCookies();
            return Ok();
        }
        
        [HttpGet("me")]
        public IActionResult GetUserData()
        {
            logger.Information("Поступил запрос на api/auth/me");
            logger.Information("Request Headers: {Headers}", string.Join(", ", Request.Headers.Select(h => $"{h.Key}: {h.Value}")));
            logger.Information("Request Cookies: {Cookies}", string.Join(", ", Request.Cookies.Select(c => $"{c.Key}: {c.Value}")));
            
            if (Request.Cookies.TryGetValue(JwtCookieName, out var jwt))
            {
                var result = authService.GetCurrentUserData(jwt);
                if (result.ErrorCode == ErrorCode.Unauthorized)
                    return Unauthorized("Неверные или просроченный jwt токен");
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);
                return Ok(result.Data);    
            }
            
            logger.Information("Нет подходящего куки для авторизации");
            return Unauthorized();
        }
        
        [HttpGet("health")]
        public IActionResult Health()
        {
            logger.Information("Health check request received");
            logger.Information("Request Headers: {Headers}", string.Join(", ", Request.Headers.Select(h => $"{h.Key}: {h.Value}")));
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        private CookieOptions GetTokenCookieOptions(int expiresInMinutes)
        {
            if (expiresInMinutes <= 0)
                throw new ArgumentOutOfRangeException();
            var result = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true, // Required for SameSite=None
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            };
            return result;
        }
        private CookieOptions GetTokenCookieOptions()
        {
            var result = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true, // Required for SameSite=None
            };
            return result;
        }
        
        private string? GetIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private void DeleteCookies()
        {
            Response.Cookies.Delete(JwtCookieName, GetTokenCookieOptions());
            Response.Cookies.Delete(RefreshTokenCookieName, GetTokenCookieOptions());
        }

        private void SetTokenCookies(TokenData accessJwtToken, TokenData refreshToken)
        {
            try
            {
                DeleteCookies();

                Response.Cookies.Append(JwtCookieName, accessJwtToken.Token,
                    GetTokenCookieOptions(accessJwtToken.ExpiresInMinutes));
                Response.Cookies.Append(RefreshTokenCookieName, refreshToken.Token,
                    GetTokenCookieOptions(refreshToken.ExpiresInMinutes));
            }
            catch (Exception ex)
            {
                logger.Information("Ошибка при обновлении куки токенов: {message}", ex.Message);
            }
        }
    }

}

