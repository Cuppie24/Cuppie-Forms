using cuppie.Data;
using cuppie.DTO;
using cuppie.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Buffers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace cuppie.Services
{
    public class AuthHandler
    {        
        private readonly IConfiguration _config;
        private readonly CuppieDbContext _dbContext;

        public static string JwtCookieName = "jwt";
        public static string JwtKeyName = "Key", 
            JwtIssuerName = "Issuer", 
            JwtAudienceName = "Audience",
            JwtSectionName = "JWT";
        public AuthHandler(CuppieDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;            
        }

        public async Task<OperationResult<User>> AddUser(RegisterModelDto registerModel)
        {
            var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == registerModel.Username);

            if (user != null)
            {
                return OperationResult<User>.Failure("Пользователь с таким именем уже существует", ErrorCode.Conflict);
            }

            CryptoService cryptoService = new();
            byte[] passSalt = cryptoService.GenerateSalt(16);
            user = new User
            {
                Username = registerModel.Username,
                Salt = passSalt,
                PasswordHash = cryptoService.HashPassword(registerModel.Password, passSalt),
                Email = registerModel.Email
            };
            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();
            return OperationResult<User>.Success(user);
        }

        public async Task<OperationResult<string>> IsAuthenticated(LoginModelDto loginModel)
        {
            var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == loginModel.Username);

            if (user == null)
            {
                return OperationResult<string>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
            }

            CryptoService cryptoService = new();

            // Проверяем пароль
            bool isPasswordValid = cryptoService.VerifyPassword(loginModel.Password, user.PasswordHash, user.Salt);

            if (!isPasswordValid)
            {
                return OperationResult<string>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
            }

            var jwtToken = GenerateJwtToken(user);
            if (string.IsNullOrEmpty(jwtToken))
            {
                Console.WriteLine("Error: JWT токен пустой");
                return OperationResult<string>.Failure("Не удалось сгенерировать JWT токен", ErrorCode.InternalServerError);
            }
            Console.WriteLine($"info: Сгенерирован токен: {jwtToken.Length}");

            return OperationResult<string>.Success(jwtToken);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var jwtSettings = _config.GetSection("JWT");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings[JwtKeyName]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds,
                issuer: jwtSettings[JwtIssuerName],
                audience: jwtSettings[JwtAudienceName]
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
