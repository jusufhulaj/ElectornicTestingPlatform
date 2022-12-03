using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.DTOs.Question;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicTestingSystem.Models.Entities
{
    [PrimaryKey("Id")]
    public class MappedExamsAndQuestions
    {
        public int Id { get; set; }

        [ForeignKey("ExamId")]
        public int ExamId { get; set; }
        public Exam Exam { get; set; }


        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
