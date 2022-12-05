using ElectronicTestingSystem.Models.Entities;

namespace ElectronicTestingSystem.Helpers.CustomExceptions.ExamExceptions
{
    public class ExamNotFoundException : Exception
    {
        public ExamNotFoundException(int examId) : base(String.Format("Exam not found: {0}", examId))
        {

        }

        public ExamNotFoundException(string message) : base(message) { }
    }
}
