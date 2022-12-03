using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicTestingSystem.Models.Entities
{
    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
    }
}
