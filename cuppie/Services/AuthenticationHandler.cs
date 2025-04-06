using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace cuppie.Services
{
    public class AuthenticationHandler
    {
        public bool Authentication(string username, string password)
        {
            return false;
        } 
        

        //public string GenerateJwtToken(string username)
        //{
        //    var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
        //    // создаем JWT-токен
        //    var jwt = new JwtSecurityToken(
        //            issuer: AuthOptions.ISSUER,
        //    audience: AuthOptions.AUDIENCE,
        //    claims: claims,
        //            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
        //            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        //    return new JwtSecurityTokenHandler().WriteToken(jwt);
        //}
    }

    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "MyAuthClient"; // потребитель токена
        const string KEY = "mysupersecret_secretsecretsecretkey!123";   // ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
