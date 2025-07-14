namespace Cuppie.Domain.Entities;

public class RefreshTokenEntity
{
    public int Id { get; init; }
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public string CreatedByIp { get; set; } = null!;
    public string? RevokedByIp { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset RevokedAt { get; set; }
    public DateTimeOffset Expires { get; set; }
    
   

}