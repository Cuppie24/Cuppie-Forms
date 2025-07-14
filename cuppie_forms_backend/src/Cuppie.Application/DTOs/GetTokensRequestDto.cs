namespace Cuppie.Application.DTOs;

public class GetTokensRequestDto
{
    public string? OldRefreshToken { get; set; }
    public string Ip { get; set; } = null!;
    public int UserId { get; set; }
}