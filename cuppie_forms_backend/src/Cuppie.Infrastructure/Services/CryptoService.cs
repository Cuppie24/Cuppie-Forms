using System.Security.Cryptography;
using System.Text;
using Cuppie.Application.Interfaces.Services;

namespace Cuppie.Infrastructure.Services
{
    public class CryptoService : ICryptoService
    {        
        public string HashPassword(string password, byte[] salt)
        {
            byte[] result;
            using (SHA256 sHa256 = SHA256.Create())
            {                
                result = sHa256.ComputeHash(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));                
            }
            return Convert.ToBase64String(result);
        }

        public bool VerifyPassword(string password, string passHash, byte[] passSalt)
        {
            return HashPassword(password, passSalt) == passHash;
        }

        public byte[] GenerateSalt(int size=16)
        {
            byte[] salt = new byte[size];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }
    }
}
