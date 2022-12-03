using ElectronicTestingSystem.Models.DTOs.Answer;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;

namespace ElectronicTestingSystem.Services.IService
{
    public interface IAnswerService
    {
        Task CreateAnswer(AnswerCreateDTO answerToCreate);
        Task<Answer> GetAnswer(int answerId);
        Task UpdateAnswer(Answer answer);
        Task DeleteAnswer(int answerdId);

        Task<List<Answer>> GetAllAnswers();
    }
}
