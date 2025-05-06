using System.ComponentModel.DataAnnotations;

namespace Cuppie.Application.DTOs
{
    public class LoginModelDto
    {
        public LoginModelDto(string username, string password)
        {
            Username = username;
            Password = password;
        }

        [Required]
        [StringLength(30)]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    public class RegisterModelDto
    {
        public RegisterModelDto(string username, string password, string email)
        {
            Username = username;
            Email = email;
            Password = password;
        }
        [Required]
        [StringLength(30)]
        public string? Username { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Введите корректный email")]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }
    }
    
    public class SafeUserDataDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
