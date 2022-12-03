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

        public async Task CreateExam(int numOfQuestions)
        {
            Exam exam = new Exam { NumberOfQuestions = numOfQuestions, TotalPoints = 0};
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
        }

        public async Task<ExamDTO> GetExam(int examId)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetById(e => e.Id == examId).AsNoTracking().FirstOrDefaultAsync();

            var ExamWithQuestions = await _examHelperMethods.GetExamQuestions(_unitOfWork, _mapper, exam);

            return ExamWithQuestions;
        }

        public async Task<List<ExamDTO>> GetAllExams()
        {
            var exams = await _unitOfWork.Repository<Exam>().GetAll().ToListAsync();

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

            var examQuestions = await _examHelperMethods.GetMappedExams(_unitOfWork, examId);
            _unitOfWork.Repository<MappedExamsAndQuestions>().DeleteRange(examQuestions.ToList());

            _unitOfWork.Repository<Exam>().Delete(exam);
            _unitOfWork.Complete();
        }

        /*public async Task UpdateExam(ExamDTO examToUpdate)
        {
            var exams = await _unitOfWork.Repository<Exam>().GetByCondition(x => x.Id == examToUpdate.Id).GroupBy(exam => new { exam.Id, exam }).ToListAsync();

            
        }*/
        public async Task UpdateExam(ExamDTO examToUpdate)
        {
            Console.WriteLine("Update Exam");
        }

        public async Task<string> RequestExam(IdentityUser user, int examId)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetById(e => e.Id == examId).FirstOrDefaultAsync();

            var existingRequest = await _unitOfWork.Repository<RequestedExams>().GetByCondition(x => x.ExamId == examId && x.UserId == user.Id)?.FirstOrDefaultAsync();
            
            if(existingRequest == null)
            {
                RequestedExams requestedExam = new RequestedExams
                {
                    Exam = exam,
                    ExamId = examId,

                    User = user,
                    UserId = user.Id

                };

                _unitOfWork.Repository<RequestedExams>().Create(requestedExam);
                _unitOfWork.Complete();

                return "Request created successfully!";
            }
            return "A request has already been issued!";
        }

        public async Task<string> ApproveExam(IdentityUser user, int examId)
        {
            var requestedExam = await _unitOfWork.Repository<RequestedExams>().GetByCondition(x => x.UserId == user.Id && x.ExamId == examId)?.FirstOrDefaultAsync();

            if (requestedExam == null)
            {
                return "Invalid Request";
            }

            requestedExam.Status = "Approved";
            _unitOfWork.Repository<RequestedExams>().Update(requestedExam);
            _unitOfWork.Complete();

            return $"Approved exam {examId} for user {user.UserName}!";
        }

        public async Task<ExamResultsDTO> CheckExamSubmit(string userId, int id, List<ExamSubmitDTO> userAnswers)
        {
            var exam = await GetExam(id);

            if (exam == null)
            {
                return new ExamResultsDTO
                {
                    Error = "Could not find a particular exam with the given Id!"
                };
            }

            var userExamRequest = await _unitOfWork.Repository<RequestedExams>().GetByCondition(e => e.UserId == userId && e.ExamId == exam.Id).FirstOrDefaultAsync();
            if(userExamRequest == null)
            {
                return new ExamResultsDTO
                {
                    Error = "Make sure you have made a request for this exam before trying to participate!"
                };
            }
            else if(userExamRequest.Status == "Processing")
            {
                return new ExamResultsDTO
                {
                    Error = "Your request for taking this exam has not been approved yet!"
                };
            }else if(userExamRequest.Status == "Declined")
            {
                return new ExamResultsDTO
                {
                    Error = "Your request for taking this exam has been declined!"
                };
            }else if(userExamRequest.Status == "Done")
            {
                return new ExamResultsDTO
                {
                    Error = "You have already taken this exam. Try another one!"
                };
            }
            
            var examQuestions = await _examHelperMethods.GetExamQuestions(_unitOfWork, _mapper, _mapper.Map<Exam>(exam));
            
            if(userAnswers.Count != examQuestions.Questions.Count)
            {
                return new ExamResultsDTO
                {
                    Error = "You have not answered all the questions corresponding to this exam!"
                };
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

            //return $"Thank you for submiting exam {examId}.\nYou have {correctAnswers.Count} correct answers with a total of {points} points.\nYou got a {grade}";
            
            // Update exam status for user after taking it
            userExamRequest.Status = "Done";
            _unitOfWork.Repository<RequestedExams>().Update(userExamRequest);
            _unitOfWork.Complete();
            
            return new ExamResultsDTO
            {
                CorrectAnswers = correctAnswers.Count,
                Grade = grade
            };
        
        }

        public async Task<Question> CheckAnswer(int questionId, string answer)
        {
            var question = await _unitOfWork.Repository<Question>().GetById(q => q.Id == questionId).FirstOrDefaultAsync();

            if (question == null)
            {
                return null;
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
