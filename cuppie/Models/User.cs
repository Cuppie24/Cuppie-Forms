using System.ComponentModel.DataAnnotations;

namespace cuppie.Models
{
    public class User
    {       
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string? Username { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Введите корректный email")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? PasswordHash { get; set; }
        [Required]
        [MaxLength(16)]
        public byte[]? Salt { get; set; }
    }
}
