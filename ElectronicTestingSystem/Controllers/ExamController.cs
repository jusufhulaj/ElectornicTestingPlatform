using ElectronicTestingSystem.Helpers;
using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("/api/exams")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ExamController(IExamService examService, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _examService = examService;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<List<Exam>>> Create(int numOfQuestions)
        {
            await _examService.CreateExam(numOfQuestions);

            return Ok("Exam created successfully!");
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<ExamDTO>> Get(int id)
        {
            var exam = await _examService.GetExam(id);

            if (exam == null)
            {
                return NotFound();
            }

            return Ok(exam);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Exam>>> GetAll()
        {
            List<ExamDTO> exams = await _examService.GetAllExams();

            if(exams == null)
            {
                return NotFound();
            }

            return Ok(exams);
        }

        [HttpPost("{id}/request")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<IActionResult> RequestAccess(int id)
        {
            var user = await _userManager.FindByIdAsync(User.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value);

            var pathToFile = "Templates/exam-request.html";
            string htmlBody = "";
            using (StreamReader streamReader = System.IO.File.OpenText(pathToFile))
            {
                htmlBody = streamReader.ReadToEnd(); 
            }

            string postForm = $"<form method=\"post\" action=\"https://localhost:7011/api/exams/{id}/approve?userId={user.Id}\"><button style=\"display: inline-block;min-width: 260px;height: 50px;margin: 0;padding: 0 16px;\r\n                                background-color: #1d79f2;border-radius: 4px;font-family: 'Source Sans Pro', sans-serif;font-size: 16px;color: #ffffff;font-weight: 500;text-decoration: none;line-height: 50px;text-align: center;\" target=\"_blank\">Approve</button></form>";
            //var postForm = "form";
            var myData = new[] { "Jusuf Hulaj", user.UserName, id.ToString(), postForm};
            var content = string.Format(htmlBody, myData);

            await _emailSender.SendEmailAsync("jusufhulaj16@gmail.com", "Exam Request", content);

            var requestResult = await _examService.RequestExam(user, id);

            return Ok(requestResult);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveExam([FromQuery]string userId, int id)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var result = await _examService.ApproveExam(user, id);

            return Ok(result);
        }

        [HttpPost("{id}/submit")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<IActionResult> SubmitExam(int id, List<ExamSubmitDTO> userAnswers)
        {
            var user = await _userManager.FindByIdAsync(User.Claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value);
            
            var result = await _examService.CheckExamSubmit(user.Id, id, userAnswers);

            if(result.Error != null)
            {
                return BadRequest(result.Error);
            }

            var pathToFile = "Templates/exam-results.html";
            string htmlBody = "";
            using (StreamReader streamReader = System.IO.File.OpenText(pathToFile))
            {
                htmlBody = streamReader.ReadToEnd();
            }

            var myData = new[] { user.UserName, id.ToString(), result.CorrectAnswers.ToString(), result.Grade.ToString() };
            var content = string.Format(htmlBody, myData);

            await _emailSender.SendEmailAsync(user.Email, "Exam Results", content);

            return Ok($"You have successfully submited your answers for exam {id}.\nMake sure to check email for your results!");
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _examService.DeleteExam(id);

            return Ok($"Succesfully delted Exam {id}.");
        }    
    }
}
