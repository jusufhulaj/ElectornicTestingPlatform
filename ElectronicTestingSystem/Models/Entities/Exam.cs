using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ElectronicTestingSystem.Models.Entities
{
    public class Exam
    {
        public int Id { get; set; }
        public int NumberOfQuestions { get; set; }
        public double TotalPoints { get; set;  }
    }
}
