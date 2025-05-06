using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces;
using Cuppie.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Ilogger = Serilog.ILogger;

namespace Cuppie.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService, Ilogger ilogger) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Ошибки валидации
            }

            try
            {
                var result = await authService.AddUser(registerDto);
                if (result.IsSuccess) return Ok(result.Data);
                else if (result.ErrorCode == ErrorCode.Conflict) return Conflict("Пользователь с таким именем уже существует");
                else return BadRequest("Произошла ошибка");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDto loginDto)
        {
            ilogger.Information("Поступил запрос на api/auth/login");
            if (!ModelState.IsValid)
            {
                ilogger.Warning("Некорректные данные для логина");
                return BadRequest(ModelState);
            }
            var result = await authService.IsAuthenticated(loginDto);

            if (result.IsSuccess)
            {
                try
                {
                    Response.Cookies.Append(JwtTokenService.JwtCookieName, result.Data ?? throw new InvalidOperationException(), new CookieOptions
                    {
                        HttpOnly = true,
                        //Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(120)
                    });
                    ilogger.Information("Логин прошел успешно, jwt куки отправлено");
                }
                catch (Exception e)
                {
                    ilogger.Error(e, "Ошибка при прикреплении jwt куки к ответу");
                    return StatusCode(500, "Ошибка при создании jwt куки");
                }

                return Ok(result.Data);
            }

            if (result.ErrorCode is ErrorCode.Unauthorized)
            {
                ilogger.Information("Неверный логин или пароль");
                return Unauthorized("Неверный логин или пароль");
            }
            return BadRequest($"Прозошла ошибка: {result.ErrorMessage}");
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                var result = authService.RefreshToken(refreshToken);
                if (result.IsSuccess)
                    return Ok(result.Data);
                else
                    return BadRequest($"Ошибка при продлении токена: {result.ErrorMessage}");
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok();
        }
        
        [HttpGet("me")]
        public IActionResult GetUserData()
        {
            ilogger.Information("Поступил запрос на api/auth/me");
            if (Request.Cookies.TryGetValue(JwtTokenService.JwtCookieName, out var jwt))
            {
                var result = authService.GetCurrentUserData(jwt);
                if (result.Data == null)
                {
                    ilogger.Warning("Не удалось получить данные из jwt куки");
                    return Unauthorized();
                }
                return Ok(result.Data);    
            }
            
            ilogger.Information("Нет подходящего куки для авторизации");
            return Unauthorized();
        }
    }

}

