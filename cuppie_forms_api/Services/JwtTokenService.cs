using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using cuppie_forms_api.DTO;
using cuppie_forms_api.Models;

namespace cuppie_forms_api.Services
{
    public class JwtTokenService
    {
        private IConfiguration _config;

        private static string jwtCookieName = "jwt",
            jwtKeyName = "Key",
            jwtIssuerName = "Issuer",
            jwtAudienceName = "Audience",
            jwtLifeTimeName = "LifeTime",
            jwtSectionName = "JWT";
        private static TokenValidationParameters validationParameters;
        public static string JwtCookieName { get => jwtCookieName; set => jwtCookieName = value; }
        public static string JwtKeyName { get => jwtKeyName; set => jwtKeyName = value; }
        public static string JwtIssuerName { get => jwtIssuerName; set => jwtIssuerName = value; }
        public static string JwtAudienceName { get => jwtAudienceName; set => jwtAudienceName = value; }
        public static string JwtLifeTimeName { get => jwtLifeTimeName; set => jwtLifeTimeName = value; }
        public static string JwtSectionName { get => jwtSectionName; set => jwtSectionName = value; }
        public static TokenValidationParameters ValidationParameters { get => validationParameters; set => validationParameters = value; }






        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var jwtSettings = _config.GetSection(JwtSectionName);
            if (!int.TryParse(jwtSettings[JwtLifeTimeName], out int lifeTime))
            {
                throw new InvalidDataException("Некорректное значение времени жизни токена в конфиге");
            }

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(lifeTime),
                    signingCredentials: GetSigningCredentials(),
                    issuer: jwtSettings[JwtIssuerName],
                    audience: jwtSettings[JwtAudienceName]
                    );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private string GenerateJwtToken(Claim[] claims)
        {
            var jwtSettings = _config.GetSection(JwtSectionName);
            if (!int.TryParse(jwtSettings[JwtLifeTimeName], out int lifeTime))
            {
                throw new InvalidDataException("Некорректное значение времени жизни токена в конфиге");
            }

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: GetSigningCredentials(),
                issuer: jwtSettings[JwtIssuerName],
                audience: jwtSettings[JwtAudienceName]
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _config.GetSection(JwtSectionName);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings[JwtKeyName]));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }


        public OperationResult<string> RefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }


                var name = principal.Identity?.Name;
                var nameId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(nameId))
                    return OperationResult<string>.Failure("Некорректные данные для обновления токена", ErrorCode.BadRequest);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.NameIdentifier, nameId)
                };


                var updatedToken = GenerateJwtToken(claims);
                return OperationResult<string>.Success(updatedToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"error: Ошибка при обновлении токена: {e.Message}\r\n{e.StackTrace}");
                return OperationResult<string>.Failure($"Произошла непредвиденная ошибка: {e.Message}", ErrorCode.InternalServerError);
            }
        }

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        static JwtTokenService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var jwtSettings = config.GetSection(JwtSectionName);
            validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings[JwtIssuerName],
                ValidateAudience = true,
                ValidAudience = jwtSettings[JwtAudienceName],
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings[JwtKeyName])),
                ValidateIssuerSigningKey = true,
            };
        }
    }
}
