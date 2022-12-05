using ElectronicTestingSystem.Models.DTOs.Question;

namespace ElectronicTestingSystem.Models.DTOs.Exam
{
    public class ExamDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int NumberOfQuestions { get; set; }
        public double TotalPoints { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}
