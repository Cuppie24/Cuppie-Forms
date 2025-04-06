using System.Security.Cryptography;
using System.Text;
using cuppie.API;

namespace cuppie.Services
{
    public class CryptoService
    {        
        public string HashPassword(string password, byte[] salt)
        {
            byte[] result;
            using (SHA256 sHA256 = SHA256.Create())
            {                
                result = sHA256.ComputeHash(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));                
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
