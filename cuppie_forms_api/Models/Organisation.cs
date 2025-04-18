using System.ComponentModel.DataAnnotations;

namespace cuppie_forms_api.Models
{
    public class Organisation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength =3)]
        public string? Name { get; set; }

        [Required]
        public DateOnly CreatedAt { get; set; }

        public ICollection<UserOrganisation> UserOrganisations { get; set; } = new List<UserOrganisation>();
    }
}
