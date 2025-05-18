using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces;
using Cuppie.Domain.Entities;
using Cuppie.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cuppie.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        public static string JwtCookieName { get; set; } = "jwt";
        public static string JwtKeyName { get; set; } = "Key";
        public static string JwtIssuerName { get; set; } = "Issuer";
        public static string JwtAudienceName { get; set; } = "Audience";
        public static string JwtLifeTimeName { get; set; } = "LifeTime";
        public static string JwtSectionName { get; set; } = "JWT";

        public TokenValidationParameters ValidationParameters { get; set; }
        
        private readonly JwtOptions jwtOptions;


        public string GenerateJwtToken(UserEntity userEntity)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userEntity.Username),
                new Claim(ClaimTypes.NameIdentifier, userEntity.Id.ToString())
            };
            
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationInMinutes),
                signingCredentials: GetSigningCredentials(),
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private string GenerateJwtToken(Claim[] claims)
        {

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationInMinutes),
                signingCredentials: GetSigningCredentials(),
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
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

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            jwtOptions = options.Value; 
            
            ValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ValidateIssuerSigningKey = true,
            };
        }
    }
}
