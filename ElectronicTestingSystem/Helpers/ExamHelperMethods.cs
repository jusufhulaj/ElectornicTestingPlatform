using AutoMapper;
using ElectronicTestingSystem.Data.UnitOfWork;
using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicTestingSystem.Helpers
{
    public class ExamHelperMethods
    {
        public async Task<ExamDTO> GetExamQuestions(IUnitOfWork unitOfWork, IMapper mapper, Exam exam)
        {
            var ExamQuestions = await unitOfWork.Repository<MappedExamsAndQuestions>().GetAll().Where(e => e.ExamId == exam.Id)
                                                                  .Select(eq => mapper.Map<QuestionDTO>(eq.Question)).ToListAsync();

            ExamDTO examDTO = new ExamDTO
            {
                Id = exam.Id,
                NumberOfQuestions = exam.NumberOfQuestions,
                TotalPoints = exam.TotalPoints,
                Questions = ExamQuestions

            };

            return examDTO;
        }

        public async Task<List<MappedExamsAndQuestions>> GetMappedExams(IUnitOfWork unitOfWork, int id)
        {
            var MappedExams = await unitOfWork.Repository<MappedExamsAndQuestions>().GetByCondition(me => me.ExamId == id).ToListAsync();

            return MappedExams;
        }
    }
}
