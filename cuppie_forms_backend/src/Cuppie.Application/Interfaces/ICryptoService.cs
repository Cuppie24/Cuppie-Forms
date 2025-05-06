namespace Cuppie.Application.Interfaces;

public interface ICryptoService
{
    string HashPassword(string password, byte[] salt);
    bool VerifyPassword(string password, string passHash, byte[] passSalt);
    byte[] GenerateSalt(int size=16);
}