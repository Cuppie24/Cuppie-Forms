using System.ComponentModel.DataAnnotations;

namespace Cuppie.Domain.Entities
{
    public class UserEntity
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

        public ICollection<UserOrganisationEntity> UserOrganisations { get; set; } = new List<UserOrganisationEntity>();
    }
}
