using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("/api/questions")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpPut]
        public async Task<ActionResult> Create(List<QuestionCreateDTO> questionsToCreate)
        {
            foreach (var question in questionsToCreate)
            {
                await _questionService.CreateQuestion(question);
            }

            return Ok("Questions created succesfully!!");
        }

        [HttpPost("addFromFile")]
        public async Task<IActionResult> CreateFromFile(IFormFile file)
        {
            if (file == null || file.Length < 1)
            {
                return BadRequest("Invalid File!");
            }

            string fileExtension = Path.GetExtension(file.FileName);

            if (fileExtension == ".csv")
            {
                await _questionService.CreateFromCSV(file);
            }
            else if (fileExtension == ".json")
            {
                await _questionService.CreateFromJSON(file);
            }
            else if (fileExtension == ".xml")
            {
                await _questionService.CreateFromXML(file);
            }
            else
            {
                return BadRequest("Could not read data from the chosen file!");
            }

            return Ok();
        }

        /*[HttpPost("fileName")]
        public async Task<IActionResult> CreateFromFile(string fileName)
        {
            if (fileName == null || fileName.Length < 1)
            {
                return BadRequest("Invalid File!");
            }

            string fileExtension = Path.GetExtension(fileName);

            if (fileExtension == ".csv")
            {
                await _questionService.CreateFromCSV(@"C:\Users\pan1c\Desktop\" + fileName);
            }
            else if (fileExtension == ".json")
            {
                await _questionService.CreateFromJSON(@"C:\Users\pan1c\Desktop\" + fileName);
            }
            else if (fileExtension == ".xml")
            {
                await _questionService.CreateFromXML(@"C:\Users\pan1c\Desktop\" + fileName);
            }
            else
            {
                return BadRequest("Could not read data from the chosen file!");
            }

            return Ok();
        }*/

        [HttpGet("{id}")]
        public async Task<ActionResult<List<QuestionDTO>>> Get(int id)
        {
            var question = await _questionService.Get(id);

            if(question == null)
            {
                return NotFound();
            }

            return Ok(question);
        }

        [HttpGet]
        public async Task<ActionResult<List<QuestionDTO>>> GetAll()
        {
            var questions = await _questionService.GetAll();

            if (questions == null)
            {
                return NotFound();
            }

            return Ok(questions);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _questionService.DeleteQuestion(id);

            return Ok("Question deleted successfully!");
        }
    }
}
