using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ElectronicTestingSystem.Services.IService
{
    public interface IExamService
    {
        Task<ExamDTO> CreateExam(string examTitle, string author, int numOfQuestions);
        Task<ExamDTO> GetExam(int examId);
        Task UpdateExam(ExamDTO examToUpdate);
        Task DeleteExam(int examId);

        Task<List<ExamDTO>> GetAllExams();

        Task<ExamRequestDTO> RequestExam(IdentityUser user, int examId);
        Task<string> ApproveExam(IdentityUser user, int examId);
        Task<bool> CheckApprovalStatus(string userId, int examId);

        Task<ExamResultsDTO> CheckExamSubmit(string userId, int id, List<ExamSubmitDTO> userAnswers);
    }
}
