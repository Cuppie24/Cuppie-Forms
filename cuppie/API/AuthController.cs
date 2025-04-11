using cuppie.Data;
using cuppie.DTO;
using cuppie.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace cuppie.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthHandler _authHandler;
        public AuthController(AuthHandler authenticationHandler)
        {
            _authHandler = authenticationHandler;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Ошибки валидации
            }

            try
            {
                var result = await _authHandler.AddUser(registerDto);
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
            var result = await _authHandler.IsAuthenticated(loginDto);

            if (result.IsSuccess)
            {
                try
                {
                    Response.Cookies.Append(AuthHandler.JwtCookieName, result.Data, new CookieOptions
                    {
                        HttpOnly = true,
                        //Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(120)
                    });
                    Console.WriteLine($"info: Запись jwt токена в куки: {result.Data.Length}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка при попытке записать куки с JWT: {e.Message}");
                    Console.WriteLine($"Stack Trace: {e.StackTrace}");
                    return StatusCode(500, "Ошибка при создании jwt куки");
                }

                return Ok(result.Data);
            }
            else if (result.ErrorCode is ErrorCode.Unauthorized) return Unauthorized("Неверный логин или пароль");
            else return BadRequest($"Прозошла ошибка: {result.ErrorMessage}");
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok();
        }
    }

}

