namespace cuppie.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? PassHash { get; set; }
        public string? PassSalt { get; set; }
    }
}
