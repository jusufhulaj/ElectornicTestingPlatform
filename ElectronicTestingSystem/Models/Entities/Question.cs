using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ElectronicTestingSystem.Models.Entities
{
    public class Question
    {
        public int Id { get; set; }
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
        public Answer CorrectAnswer { get; set; }
    }
}
