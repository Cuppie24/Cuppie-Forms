namespace Cuppie.Application.DTOs;

public class AuthRequestDto
{
    public string Username { get; set; }  
    public string Password { get; set; } 
    public string? Email { get; set; }
    public string Ip { get; set; } = null!;
    public string? OldRefreshToken { get; set; }
}