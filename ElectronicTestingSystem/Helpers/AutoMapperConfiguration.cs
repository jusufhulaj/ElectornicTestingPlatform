using AutoMapper;
using ElectronicTestingSystem.Models.DTOs;
using ElectronicTestingSystem.Models.DTOs.Answer;
using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace ElectronicTestingSystem.Helpers
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration() 
        {

            CreateMap<User, UserRegisterDTO>().ReverseMap();
            CreateMap<User, IdentityUser>().ReverseMap();

            CreateMap<Exam, ExamDTO>().ReverseMap();
            CreateMap<Exam, ExamUserDTO>().ReverseMap();

            CreateMap<Question, QuestionCreateDTO>().ReverseMap();
            CreateMap<Question, QuestionDTO>().ReverseMap();

            CreateMap<Answer, AnswerCreateDTO>().ReverseMap();
        }
    }
}
