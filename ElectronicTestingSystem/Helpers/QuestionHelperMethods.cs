using ElectronicTestingSystem.Data.UnitOfWork;
using ElectronicTestingSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicTestingSystem.Helpers
{
    public class QuestionHelperMethods
    {
        public async Task<Question> GetQuesiton(IUnitOfWork unitOfWork, int questionId)
        {
            Question question = await unitOfWork.Repository<Question>().GetById(question => question.Id == questionId).FirstOrDefaultAsync();

            return question;
        }
    }
}
