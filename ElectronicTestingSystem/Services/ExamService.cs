using ElectronicTestingSystem.Data.UnitOfWork;
using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using ElectronicTestingSystem.Helpers;
using Microsoft.AspNetCore.Identity;
using ElectronicTestingSystem.Models;
using ElectronicTestingSystem.Helpers.CustomExceptions.ExamExceptions;
using ElectronicTestingSystem.Helpers.CustomExceptions.AnswerExceptions;
using ElectronicTestingSystem.Helpers.CustomExceptions.QuestionExceptions;

namespace ElectronicTestingSystem.Services
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly ExamHelperMethods _examHelperMethods;

        public ExamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

            _examHelperMethods = new ExamHelperMethods();
        }

        public async Task<ExamDTO> CreateExam(string examTitle, string author, int numOfQuestions)
        {
            Exam exam = new Exam { Title = examTitle, Author = author, NumberOfQuestions = numOfQuestions, TotalPoints = 0};
            var questions = _unitOfWork.Repository<Question>().GetAll().OrderBy(q => Guid.NewGuid()).Take(numOfQuestions);

            foreach (var question in questions)
            {
                exam.TotalPoints += question.Points;

                MappedExamsAndQuestions mappedExamsAndQuestions = new MappedExamsAndQuestions
                {
                    Exam = exam,
                    ExamId = exam.Id,

                    Question = question,
                    QuestionId = question.Id
                };

                _unitOfWork.Repository<MappedExamsAndQuestions>().Create(mappedExamsAndQuestions);
            }

            _unitOfWork.Repository<Exam>().Create(exam);
            _unitOfWork.Complete();

            ExamDTO examWithQuestions = await _examHelperMethods.GetExamQuestions(_unitOfWork, _mapper, exam);
            return examWithQuestions;
        }

        public async Task<ExamDTO> GetExam(int examId)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetById(e => e.Id == examId).AsNoTracking().FirstOrDefaultAsync();

            if(exam == null)
            {
                throw new ExamNotFoundException(examId);
            }

            var ExamWithQuestions = await _examHelperMethods.GetExamQuestions(_unitOfWork, _mapper, exam);

            return ExamWithQuestions;
        }

        public async Task<List<ExamDTO>> GetAllExams()
        {
            var exams = await _unitOfWork.Repository<Exam>().GetAll().ToListAsync();

            if(exams == null)
            {
                throw new ExamNotFoundException("No exams could be found!");
            }
            
            List<ExamDTO> ExamsWithQuestions = new();
            foreach (Exam exam in exams)
            {
                ExamsWithQuestions.Add(await _examHelperMethods.GetExamQuestions(_unitOfWork, _mapper, exam));
            }

            return ExamsWithQuestions;
        }

        public async Task DeleteExam(int examId)
        {
            var exam = _mapper.Map<Exam>(await GetExam(examId));

            if(exam == null)
            {
                throw new ExamNotFoundException(examId);
            }

            var examQuestions = await _examHelperMethods.GetMappedExams(_unitOfWork, examId);
            _unitOfWork.Repository<MappedExamsAndQuestions>().DeleteRange(examQuestions.ToList());

            _unitOfWork.Repository<Exam>().Delete(exam);
            _unitOfWork.Complete();
        }

        public async Task UpdateExam(ExamDTO examToUpdate)
        {
            throw new NotImplementedException();
        }

        public async Task<ExamRequestDTO> RequestExam(IdentityUser user, int examId)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetById(e => e.Id == examId).FirstOrDefaultAsync();

            if(exam == null)
            {
                throw new ExamNotFoundException(examId);
            }

            var existingRequest = await _unitOfWork.Repository<RequestedExams>().GetByCondition(x => x.ExamId == examId && x.UserId == user.Id)?.FirstOrDefaultAsync();

            if (existingRequest == null)
            {
                RequestedExams requestedExam = new RequestedExams
                {
                    Exam = exam,
                    ExamId = examId,

                    User = user,
                    UserId = user.Id,

                    Status = "Requested"

                };

                _unitOfWork.Repository<RequestedExams>().Create(requestedExam);
                _unitOfWork.Complete();

                return new ExamRequestDTO
                {
                    Admin = "Jusuf Hulaj",
                    User = user.UserName,
                    ExamId = examId
                };
            }else if (existingRequest.Status == "Done")
            {
                throw new ExamRequestException($"You have already taken exam {exam.Id}");
            }

            throw new ExamRequestException($"You have already made a request for exam {exam.Id}");
        }

        public async Task<string> ApproveExam(IdentityUser user, int examId)
        {
            var requestedExam = await _unitOfWork.Repository<RequestedExams>().GetByCondition(x => x.UserId == user.Id && x.ExamId == examId)?.FirstOrDefaultAsync();

            if (requestedExam == null)
            {
                throw new ExamRequestException("You must first make a request for this exam!");
            }

            requestedExam.Status = "Approved";

            _unitOfWork.Repository<RequestedExams>().Update(requestedExam);
            _unitOfWork.Complete();

            return $"Approved exam {examId} for user {user.UserName}!";
        }

        public async Task<bool> CheckApprovalStatus(string userId, int examId)
        {
            return await _examHelperMethods.ApprovedExam(_unitOfWork, userId, examId);
        }

        public async Task<ExamResultsDTO> CheckExamSubmit(string userId, int id, List<ExamSubmitDTO> userAnswers)
        {
            var exam = await GetExam(id);

            if (exam == null)
            {
                throw new ExamNotFoundException(id);
            }

            var userExamRequest = await _unitOfWork.Repository<RequestedExams>().GetByCondition(e => e.UserId == userId && e.ExamId == exam.Id).FirstOrDefaultAsync();
            if(userExamRequest == null)
            {
                throw new ExamRequestException("Make sure you have made a request for this exam and that it has been approved!");
            }else if(userExamRequest.Status == "Requested")
            {
                throw new ExamRequestException("Your request for this exam has not been approved yet!");
            }else if(userExamRequest.Status == "Declined")
            {
                throw new ExamRequestException("Your request for this exam has been declined!");
            }else if(userExamRequest.Status == "Done")
            {
                throw new ExamRequestException("You have already taken this exam! Maybe try another one.");
            }
            
            var examQuestions = await _examHelperMethods.GetExamQuestions(_unitOfWork, _mapper, _mapper.Map<Exam>(exam));
            
            if(userAnswers.Count != examQuestions.Questions.Count)
            {
                throw new AnswerException("You have not answered all the questions corresponding to this exam!");
            }

            Dictionary<int, bool> correctAnswers = new Dictionary<int, bool>();
            double points = 0;

            foreach(ExamSubmitDTO answer in userAnswers)
            {
                var correctAnswer = await CheckAnswer(answer.QuestionId, answer.Answer);
                if(correctAnswer != null)
                {
                    points += correctAnswer.Points;
                    correctAnswers.Add(correctAnswer.Id, true);
                }
            }

            var resultPercentage = (points / exam.TotalPoints) * 100;
            byte grade = 5;

            if(resultPercentage >= 50 && resultPercentage < 60)
            {
                grade = 6;
            }else if(resultPercentage >= 60 && resultPercentage < 70)
            {
                grade = 7;
            }
            else if (resultPercentage >= 70 && resultPercentage < 80)
            {
                grade = 8;
            }   
            else if (resultPercentage >= 80 && resultPercentage < 90)
            {
                grade = 9;
            }
            else if (resultPercentage >= 90 && resultPercentage <= 100)
            {
                grade = 10;
            }

            // Update exam status for user after taking it
            userExamRequest.Status = "Done";
            _unitOfWork.Repository<RequestedExams>().Update(userExamRequest);
            _unitOfWork.Complete();

            return new ExamResultsDTO
            {
                CorrectAnswers = correctAnswers.Count,
                Points = points,
                Grade = grade
            };
        
        }

        public async Task<Question> CheckAnswer(int questionId, string answer)
        {
            var question = await _unitOfWork.Repository<Question>().GetById(q => q.Id == questionId).FirstOrDefaultAsync();

            if (question == null)
            {
                throw new QuestionNotFoundException(questionId);
            }

            var questionAnswer = await _unitOfWork.Repository<Answer>().GetByCondition(a => a.QuestionId == questionId && a.Text == answer).FirstOrDefaultAsync();

            if(questionAnswer == null)
            {
                return null;
            }
            
            if (question.CorrectAnswer == questionAnswer)
            {
                return question;
            };

            return null;
        }
    }
}
