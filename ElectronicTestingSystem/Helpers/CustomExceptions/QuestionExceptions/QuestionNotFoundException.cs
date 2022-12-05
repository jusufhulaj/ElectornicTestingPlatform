namespace ElectronicTestingSystem.Helpers.CustomExceptions.QuestionExceptions
{
    public class QuestionNotFoundException : Exception
    {
        public QuestionNotFoundException(int questionId) :base(String.Format("Question not found: {0}", questionId)) 
        {
        
        }

        public QuestionNotFoundException(string message) : base(message)
        {

        }
    }
}
