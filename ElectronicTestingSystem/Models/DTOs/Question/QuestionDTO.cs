namespace ElectronicTestingSystem.Models.DTOs.Question
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public double Points { get; set; }

        // Question Options
        public string FirstOption { get; set; }
        public string SecondOption { get; set; }
        public string ThirdOption { get; set; }
        public string FourthOption { get; set; }
    }
}
