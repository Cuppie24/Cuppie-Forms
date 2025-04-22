using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using cuppie_forms_api.DTO;
using cuppie_forms_api.Models;

namespace cuppie_forms_api.Services
{
    public class JwtTokenService(IConfiguration config)
    {
        public static string JwtCookieName { get; set; } = "jwt";
        public static string JwtKeyName { get; set; } = "Key";
        public static string JwtIssuerName { get; set; } = "Issuer";
        public static string JwtAudienceName { get; set; } = "Audience";
        public static string JwtLifeTimeName { get; set; } = "LifeTime";
        public static string JwtSectionName { get; set; } = "JWT";
        public static TokenValidationParameters ValidationParameters { get; set; }


        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var jwtSettings = config.GetSection(JwtSectionName);
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
            var jwtSettings = config.GetSection(JwtSectionName);
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
            var jwtSettings = config.GetSection(JwtSectionName);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings[JwtKeyName]));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }


        public OperationResult<string> RefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, ValidationParameters, out SecurityToken validatedToken);
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

        public ClaimsPrincipal ExtractClaimsPrincipal(string token)
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters, out SecurityToken validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }

        static JwtTokenService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var jwtSettings = config.GetSection(JwtSectionName);
            ValidationParameters = new TokenValidationParameters
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
