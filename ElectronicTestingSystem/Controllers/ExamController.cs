using ElectronicTestingSystem.Helpers;
using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ElectronicTestingSystem.Helpers.CustomExceptions.ExamExceptions;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("/api/exams")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExamNotFoundException> _logger; 

        public ExamController(IExamService examService, UserManager<IdentityUser> userManager, IEmailSender emailSender, ILogger<ExamNotFoundException> logger)
        {
            _examService = examService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<ExamDTO>> Create(string examTitle, string author, int numOfQuestions)
        {
            var newExam = await _examService.CreateExam(examTitle, author, numOfQuestions);

            if(newExam == null)
            {
                return BadRequest("Exam could not be created!");
            }

            return Ok(newExam);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, User")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var exam = await _examService.GetExam(id);

                if (exam == null)
                {
                    return NotFound();
                }

                if (User.IsInRole("User"))
                {

                    var approvedExam = await _examService.CheckApprovalStatus(User.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value, id);

                    if (!approvedExam)
                    {
                        return Ok(new ExamUserDTO
                        {
                            Id = exam.Id,
                            Title = exam.Title,
                            Author = exam.Author
                        });
                    }
                }

                return Ok(exam);
            }catch(ExamNotFoundException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, User")]
        public async Task<ActionResult<List<Exam>>> GetAll()
        {
            try
            {
                List<ExamDTO> exams = await _examService.GetAllExams();

                if (exams == null)
                {
                    return NotFound();
                }

                if (User.IsInRole("User"))
                {
                    List<object> examUserDTO = new List<object>();

                    foreach (var exam in exams)
                    {
                        var approvedExam = await _examService.CheckApprovalStatus(User.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value, exam.Id);

                        if (!approvedExam)
                        {
                            examUserDTO.Add(new ExamUserDTO
                            {
                                Id = exam.Id,
                                Title = exam.Title,
                                Author = exam.Author
                            });
                        }
                        else
                        {
                            examUserDTO.Add(exam);
                        }
                    }

                    return Ok(examUserDTO);
                }

                return Ok(exams);
            }catch(ExamNotFoundException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/request")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<IActionResult> RequestAccess(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value);

                var pathToFile = "Templates/exam-request.html";
                string htmlBody = "";
                using (StreamReader streamReader = System.IO.File.OpenText(pathToFile))
                {
                    htmlBody = streamReader.ReadToEnd();
                }

                string postForm = $"<form method=\"post\" action=\"https://localhost:44383/api/exams/{id}/approve?userId={user.Id}\"><button style=\"display: inline-block;min-width: 260px;height: 50px;margin: 0;padding: 0 16px;\r\n                                background-color: #1d79f2;border-radius: 4px;font-family: 'Source Sans Pro', sans-serif;font-size: 16px;color: #ffffff;font-weight: 500;text-decoration: none;line-height: 50px;text-align: center;\" target=\"_blank\">Approve</button></form>";

                var requestResult = await _examService.RequestExam(user, id);

                var myData = new[] { requestResult.Admin, requestResult.User, requestResult.ExamId.ToString(), postForm };
                var content = string.Format(htmlBody, myData);
                await _emailSender.SendEmailAsync("jusufhulaj16@gmail.com", "Exam Request", content);

                return Ok($"You have successfully sent a request to access exam {requestResult.ExamId}!");
            }catch(ExamRequestException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveExam([FromQuery]string userId, int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                var result = await _examService.ApproveExam(user, id);

                return Ok(result);
            }catch(ExamRequestException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/submit")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<IActionResult> SubmitExam(int id, List<ExamSubmitDTO> userAnswers)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.Claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value);

                var result = await _examService.CheckExamSubmit(user.Id, id, userAnswers);

                var pathToFile = "Templates/exam-results.html";
                string htmlBody = "";
                using (StreamReader streamReader = System.IO.File.OpenText(pathToFile))
                {
                    htmlBody = streamReader.ReadToEnd();
                }

                var myData = new[] { user.UserName, id.ToString(), result.CorrectAnswers.ToString(), result.Points.ToString(), result.Grade.ToString() };
                var content = string.Format(htmlBody, myData);

                await _emailSender.SendEmailAsync(user.Email, "Exam Results", content);

                return Ok($"You have successfully submited your answers for exam {id}.\nMake sure to check email for your results!");
            }catch(Exception ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _examService.DeleteExam(id);

                return Ok($"Succesfully delted Exam {id}.");
            }catch(ExamNotFoundException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }    
    }
}
