using AutoMapper;
using ElectronicTestingSystem.Data.UnitOfWork;
using ElectronicTestingSystem.Helpers;
using ElectronicTestingSystem.Helpers.CustomExceptions.QuestionExceptions;
using ElectronicTestingSystem.Models.DTOs.Answer;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace ElectronicTestingSystem.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly QuestionHelperMethods _questionHelperMethods;

        public QuestionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _questionHelperMethods = new QuestionHelperMethods();
        }

        public async Task CreateQuestion(QuestionCreateDTO questionToCreate)
        {
            var question = _mapper.Map<Question>(questionToCreate);

            _unitOfWork.Repository<Question>().Create(question);

            _unitOfWork.Complete();
        }

        public async Task CreateFromCSV(IFormFile file)
        {
            var csvData = new List<string>();
            using(var reader = new StreamReader(file.OpenReadStream()))
            {
                while(reader.Peek() >= 0)
                {
                    csvData.Add(reader.ReadLine());
                }
            }

            List<Question> questionsToCreate = new List<Question>();
            csvData.RemoveAt(0);
            
            foreach(var line in csvData)
            {
                var delimitedLine = line.Split(",");
                
                QuestionCreateDTO question = new QuestionCreateDTO
                {
                    Text = delimitedLine[0],
                    ImageUrl = delimitedLine[1],
                    Points = double.Parse(delimitedLine[2]),
                    FirstOption = delimitedLine[3],
                    SecondOption = delimitedLine[4],
                    ThirdOption = delimitedLine[5],
                    FourthOption = delimitedLine[6],
                    CorrectAnswer = new AnswerCreateDTO
                    {
                        Text = delimitedLine[7]
                    }
                };

                questionsToCreate.Add(_mapper.Map<Question>(question));
            }

            _unitOfWork.Repository<Question>().CreateRange(questionsToCreate);
            _unitOfWork.Complete();
        }

        public async Task CreateFromJSON(IFormFile file)
        {
            var jsonData = string.Empty;
            using(var reader = new StreamReader(file.OpenReadStream()))
            {
                jsonData = await reader.ReadToEndAsync();
            }

            var convertedData = JsonConvert.DeserializeObject<List<QuestionCreateDTO>>(jsonData);
            List<Question> questionsToCreate = new List<Question>();
            
            foreach (QuestionCreateDTO questionToCreate in convertedData)
            {
                questionsToCreate.Add(_mapper.Map<Question>(questionToCreate));
            }

            _unitOfWork.Repository<Question>().CreateRange(questionsToCreate);
            _unitOfWork.Complete();
        }

        public async Task CreateFromXML(IFormFile file)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<QuestionCreateDTO>));
            var questionsToCreate = new List<QuestionCreateDTO>();
            var mappedQuestions = new List<Question>();
            using (var reader = file.OpenReadStream())
            {
                questionsToCreate = (List<QuestionCreateDTO>)xmlSerializer.Deserialize(reader);
            }

            foreach (QuestionCreateDTO questionToCreate in questionsToCreate)
            {
                mappedQuestions.Add(_mapper.Map<Question>(questionToCreate));
            }

            _unitOfWork.Repository<Question>().CreateRange(mappedQuestions);
            _unitOfWork.Complete();
        }

        public async Task<QuestionDTO> Get(int questionID)
        {
            var question = await _questionHelperMethods.GetQuesiton(_unitOfWork, questionID);

            if (question == null)
            {
                throw new QuestionNotFoundException(questionID);
            }

            return _mapper.Map<QuestionDTO>(question);
        }

        public async Task<List<QuestionDTO>> GetAll()
        {
            var questions = _unitOfWork.Repository<Question>().GetAll().Select(question => _mapper.Map<QuestionDTO>(question));

            if(questions == null)
            {
                throw new QuestionNotFoundException("No questions could be found!");
            }

            return questions.ToList();
        }
        public async Task UpdateQuestion(QuestionDTO questionToUpdate)
        {
            var question = await Get(questionToUpdate.Id);
            if(question == null)
            {
                throw new QuestionNotFoundException(questionToUpdate.Id);
            }

            question.Text = questionToUpdate.Text;

            question.FirstOption = questionToUpdate.FirstOption != null || questionToUpdate.FirstOption == "" ? questionToUpdate.FirstOption : question.FirstOption;
            question.SecondOption = questionToUpdate.SecondOption != null || questionToUpdate.SecondOption == "" ? questionToUpdate.SecondOption : question.SecondOption;
            question.ThirdOption = questionToUpdate.ThirdOption != null || questionToUpdate.ThirdOption == "" ? questionToUpdate.ThirdOption : question.ThirdOption;
            question.FourthOption = questionToUpdate.FourthOption != null || questionToUpdate.FourthOption == "" ? questionToUpdate.FourthOption : question.FourthOption;

            _unitOfWork.Repository<Question>().Update(_mapper.Map<Question>(question));
            _unitOfWork.Complete();
        }

        public async Task DeleteQuestion(int questionId)
        {
            var question = await _questionHelperMethods.GetQuesiton(_unitOfWork, questionId);
            if (question == null)
            {
                throw new QuestionNotFoundException(questionId);
            }

            var ExamsWithQuestion = await _unitOfWork.Repository<MappedExamsAndQuestions>().GetAll()
                                                .Where(q => q.QuestionId == question.Id)
                                                .Select(e => e.Exam).ToListAsync();

            foreach (Exam exam in ExamsWithQuestion)
            {
                exam.NumberOfQuestions--;
            }

            _unitOfWork.Repository<Exam>().UpdateRange(ExamsWithQuestion);
            _unitOfWork.Repository<Question>().Delete(question);

            _unitOfWork.Complete();
        }
    }
}
