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
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры, дефисы и подчеркивания")]
        public string? Username { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }
    }

    public class RegisterModelDto(string username, string password, string email)
    {
        [Required]
        [StringLength(30)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры, дефисы и подчеркивания")]
        public string Username { get; set; } = username;

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Введите корректный email")]
        public string Email { get; set; } = email;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = password;
    }
    
    public class SafeUserDataDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
