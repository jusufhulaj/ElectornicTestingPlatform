using ElectronicTestingSystem.Models.DTOs.Question;

namespace ElectronicTestingSystem.Services.IService
{
    public interface IQuestionService
    {
        Task CreateQuestion(QuestionCreateDTO questionToCreate);
        Task<QuestionDTO> Get(int questionID);
        Task UpdateQuestion(QuestionDTO questionToUpdate);
        Task DeleteQuestion(int questionId);

        Task<List<QuestionDTO>> GetAll();

        Task CreateFromCSV(IFormFile file);
        Task CreateFromJSON(IFormFile file);
        Task CreateFromXML(IFormFile file);
    }
}
