using System.ComponentModel.DataAnnotations;

namespace ElectronicTestingSystem.Models.DTOs
{
    public class UserLogInDTO
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        [MaxLength()]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [MaxLength(32)]
        public string Password { get; set; }
    }
}
