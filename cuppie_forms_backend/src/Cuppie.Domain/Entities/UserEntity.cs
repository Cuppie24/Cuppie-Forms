using System.ComponentModel.DataAnnotations;

namespace Cuppie.Domain.Entities
{
    public class UserEntity
    {       
        public int Id { get; init; }

        [Required]
        [StringLength(30, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры, дефисы и подчеркивания")]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength=6)]
        public string? Email { get; set; }

        [Required] public string PasswordHash { get; set; } = null!;

        [Required] [MaxLength(16)] public byte[] Salt { get; set; } = null!;

        public ICollection<UserOrganisationEntity> UserOrganisations { get; set; } = new List<UserOrganisationEntity>();
    }
}
