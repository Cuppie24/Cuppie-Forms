using cuppie.Data;
using cuppie.Models;
using cuppie.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace cuppie.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly CuppieDbContext _dbContext;
        public AuthController(CuppieDbContext cuppieDbContext)
        {
            _dbContext = cuppieDbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Ошибки валидации
            }
            var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == registerDto.Username);

            if (user != null)
            {
                return Conflict(new { message = "Пользователь с таким именем уже существует" });
            }

            CryptoService cryptoService = new();
            byte[] passSalt = cryptoService.GenerateSalt(16);
            user = new User
            {
                Username = registerDto.Username,
                Salt = passSalt,
                PasswordHash = cryptoService.HashPassword(registerDto.Password, passSalt),
                Email = registerDto.Email
            };
            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Находим пользователя по Username
            var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            CryptoService cryptoService = new();

            // Проверяем пароль
            bool isPasswordValid = cryptoService.VerifyPassword(loginDto.Password, user.PasswordHash, user.Salt);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            return Ok(new { message = "Успешный вход", user = user.Username });

        }
    }
    public class RegisterDto
    {
        public RegisterDto(string username, string password, string email)
        {
            Username = username;
            Email = email;
            Password = password;
        }
        [Required]
        [StringLength(30)]
        public string? Username { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Введите корректный email")]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    public class LoginDto
    {
        public LoginDto(string username, string password)
        {
            Username = username;
            Password = password;
        }

        [Required]
        [StringLength(30)]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
        
    }
}

