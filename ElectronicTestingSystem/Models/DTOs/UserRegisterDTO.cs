using System.ComponentModel.DataAnnotations;

namespace ElectronicTestingSystem.Models.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(20)]
        public string LastName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        [MaxLength()]
        public string Email { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [MaxLength(32)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; }
    }
}
