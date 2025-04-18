using cuppie_forms_api.DTO;
using cuppie_forms_api.Services;
using Microsoft.AspNetCore.Mvc;
using Ilogger = Serilog.ILogger;

namespace cuppie_forms_api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AuthService authenticationHandler, Ilogger _ilogger) : ControllerBase
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
                var result = await authenticationHandler.AddUser(registerDto);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await authenticationHandler.IsAuthenticated(loginDto);

            if (result.IsSuccess)
            {
                try
                {
                    Response.Cookies.Append(JwtTokenService.JwtCookieName, result.Data, new CookieOptions
                    {
                        HttpOnly = true,
                        //Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(120)
                    });
                }
                catch (Exception e)
                {
                    _ilogger.Error(e, "Ошибка при прикреплении jwt куки к ответу");
                    return StatusCode(500, "Ошибка при создании jwt куки");
                }

                return Ok(result.Data);
            }
            else if (result.ErrorCode is ErrorCode.Unauthorized) return Unauthorized("Неверный логин или пароль");
            else return BadRequest($"Прозошла ошибка: {result.ErrorMessage}");
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                var result = authenticationHandler.RefreshToken(refreshToken);
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
    }

}

