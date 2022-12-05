using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ElectronicTestingSystem.Models.Entities
{
    public class User
    {
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        [MaxLength()]
        public string Email { get; set; }
        [Required]
        [MaxLength(16)]
        public string UserName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
