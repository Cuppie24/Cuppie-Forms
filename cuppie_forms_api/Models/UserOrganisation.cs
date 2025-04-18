using System.ComponentModel.DataAnnotations;

namespace cuppie_forms_api.Models
{
    public class UserOrganisation
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public int OrganisationId { get; set; }
        public Organisation Organisation { get; set; } = null!;

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
