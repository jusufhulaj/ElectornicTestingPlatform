using ElectronicTestingSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicTestingSystem.Models
{
    [PrimaryKey("Id")]
    public class RequestedExams
    {
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [ForeignKey("ExamId")]
        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public string Status { get; set; } = "Processing";

    }
}
