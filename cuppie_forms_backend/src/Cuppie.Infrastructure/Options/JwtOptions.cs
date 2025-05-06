namespace Cuppie.Infrastructure.Options;

public class JwtOptions
{
    public string Key { get; set; } = null!;
    public int ExpirationInMinutes { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}