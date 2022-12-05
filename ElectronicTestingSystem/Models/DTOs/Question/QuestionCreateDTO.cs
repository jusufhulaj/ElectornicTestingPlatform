using ElectronicTestingSystem.Models.DTOs.Answer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ElectronicTestingSystem.Models.DTOs.Question
{
    public class QuestionCreateDTO
    {
        public string Text { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }
        public double Points { get; set; }

        // Question Options
        public string FirstOption { get; set; }
        public string SecondOption { get; set; }
        public string ThirdOption { get; set; }
        public string FourthOption { get; set; }

        // Correct Answer
        public AnswerCreateDTO CorrectAnswer { get; set; }
    }
}
