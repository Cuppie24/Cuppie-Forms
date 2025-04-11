using System.ComponentModel.DataAnnotations;

namespace cuppie.Models
{
    public class User
    {       
        public int Id { get; set; }
        [Required]
        [StringLength(30, MinimumLength=1)]
        public string? Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength=6)]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        [MaxLength(16)]
        public byte[]? Salt { get; set; }

        public ICollection<UserOrganisation> UserOrganisations { get; set; } = new List<UserOrganisation>();
    }
}
