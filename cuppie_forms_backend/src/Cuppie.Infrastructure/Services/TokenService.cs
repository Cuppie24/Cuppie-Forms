using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces.Services;
using Cuppie.Domain.Entities;
using Cuppie.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cuppie.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        /// <summary>
        /// Represents the cookie name used to store the JWT token.
        /// This value is utilized in the HTTP response for secure transport
        /// and retrieval of JWT tokens during client-server communication.
        /// </summary>

        private TokenValidationParameters ValidationParameters { get; set; }

        /// <summary>
        /// Represents an instance of the <see cref="JwtOptions"/> class that provides
        /// configuration values used for generating and validating JWT tokens.
        /// </summary>
        /// <remarks>
        /// This variable holds JWT-related settings, including signing key, token expiration times,
        /// issuer, and audience information. It is initialized through dependency injection using
        /// the <see cref="IOptions{TOptions}"/> mechanism.
        /// </remarks>
        private readonly JwtOptions _jwtOptions;


        /// <summary>
        /// Generates a JWT access token for the specified user entity.
        /// </summary>
        /// <param name="userEntity">The user entity for which the JWT access token is to be created. Contains user-specific details such as username and ID.</param>
        /// <returns>
        /// A <see cref="TokenData"/> containing the JWT access token and its expiration time in minutes.
        /// </returns>
        public TokenData GetJwtAccessToken(UserEntity userEntity)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, userEntity.Username));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userEntity.Id.ToString()));
            if(userEntity.Email != null)
                claims.Add(new Claim(ClaimTypes.Email, userEntity.Email));
            
            
            var jwt = GenerateJwtAccessToken(claims.ToArray());
            return new TokenData(){Token = jwt, ExpiresInMinutes = _jwtOptions.AccessTokenExpiresInMinutes};
        }

        private TokenData GetJwtAccessToken(Claim[] claims)
        {
            var jwt = GenerateJwtAccessToken(claims);
            return new TokenData(){Token = jwt, ExpiresInMinutes = _jwtOptions.AccessTokenExpiresInMinutes};
        }

        /// Generates a JWT access token based on the provided claims.
        /// <param name="claims">The array of claims to include in the JWT payload.</param>
        /// <returns>A JWT access token string.</returns>
        private string GenerateJwtAccessToken(Claim[] claims)
        {
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiresInMinutes),
                signingCredentials: GetSigningCredentials(),
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        /// <summary>
        /// Generates a refresh token as a base64-encoded string with the specified size in bytes.
        /// </summary>
        /// <param name="size">The size, in bytes, of the refresh token to generate.</param>
        /// <returns>A <see cref="TokenData"/> containing the generated refresh token and its expiration duration in minutes.</returns>
        public TokenData GetRefreshToken(int size)
        {
            var tokenInBytes = new byte[size];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(tokenInBytes);   
            }

            var token = Convert.ToBase64String(tokenInBytes);
            return new TokenData(){Token = token, ExpiresInMinutes = _jwtOptions.RefreshTokenExpiresInMinutes};
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }


        public OperationResult<ClaimsPrincipal> ExtractClaimsPrincipal(string token)
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters,
                out SecurityToken validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return OperationResult<ClaimsPrincipal>.Failure("Невалидный токен", ErrorCode.Unauthorized);
            }
            return OperationResult<ClaimsPrincipal>.Success(principal);
        }

        public TokenService(IOptions<JwtOptions> options)
        {
            _jwtOptions = options.Value;
            ValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                ValidateIssuerSigningKey = true,
            };
        }
    }
}
