using AutoMapper;
using ElectronicTestingSystem.Data.UnitOfWork;
using ElectronicTestingSystem.Models.DTOs.Answer;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;

namespace ElectronicTestingSystem.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AnswerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateAnswer(AnswerCreateDTO answerToCreate)
        {
            var answer = _mapper.Map<Answer>(answerToCreate);

            _unitOfWork.Repository<Answer>().Create(answer);
            _unitOfWork.Complete();

        }

        public async Task DeleteAnswer(int answerdId)
        {
            var answer = await GetAnswer(answerdId);

            _unitOfWork.Repository<Answer>().Delete(answer);
            _unitOfWork.Complete();
        }

        public async Task<List<Answer>> GetAllAnswers()
        {
            var answers = _unitOfWork.Repository<Answer>().GetAll();

            return answers.ToList();
        }

        public async Task<Answer> GetAnswer(int answerId)
        {
            var answer = _unitOfWork.Repository<Answer>().GetById(x => x.Id == answerId).FirstOrDefault();

            return answer;

        }

        public async Task UpdateAnswer(Answer answerToUpdate)
        {
            var answer = await GetAnswer(answerToUpdate.Id);

            answer.Text = answerToUpdate.Text;

            _unitOfWork.Repository<Answer>().Update(answer);
            _unitOfWork.Complete();
        }
    }
}
