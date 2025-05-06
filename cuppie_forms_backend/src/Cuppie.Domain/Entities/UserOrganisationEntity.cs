using System.ComponentModel.DataAnnotations;

namespace Cuppie.Domain.Entities
{
    public class UserOrganisationEntity
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public UserEntity UserEntity { get; set; } = null!;

        [Required]
        public int OrganisationId { get; set; }
        public OrganisationEntity OrganisationEntity { get; set; } = null!;

        [Required]
        public DateTime JoinedAt { get; set; }

        [Required]
        public UserRoles UserRole { get; set; }
    }

    public enum UserRoles
    {
        Admin,
        User
    }
}
