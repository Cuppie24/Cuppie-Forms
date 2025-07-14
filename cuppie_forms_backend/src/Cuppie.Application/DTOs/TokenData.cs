namespace Cuppie.Application.DTOs;

public class TokenData
{
    public string Token { get; set; } = null!;
    public int ExpiresInMinutes { get; set; } // нужно чтобы передать наверх и задать время жизни куки
}