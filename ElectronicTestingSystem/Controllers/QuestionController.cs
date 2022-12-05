using Amazon.S3;
using Amazon.S3.Model;
using ElectronicTestingSystem.Helpers.CustomExceptions.QuestionExceptions;
using ElectronicTestingSystem.Models.DTOs.Question;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;
using System.Xml.Serialization;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("/api/questions")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QuestionNotFoundException> _logger;

        public QuestionController(IQuestionService questionService, IConfiguration configuration, ILogger<QuestionNotFoundException> logger)
        {
            _questionService = questionService;
            _configuration = configuration;
            _logger = logger;
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

        [HttpPut("addFromFile")]
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

        [HttpGet("{id}")]
        public async Task<ActionResult<List<QuestionDTO>>> Get(int id)
        {
            try
            {
                var question = await _questionService.Get(id);

                if (question == null)
                {
                    return NotFound();
                }

                return Ok(question);
            }catch(QuestionNotFoundException ex) {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<QuestionDTO>>> GetAll()
        {
            try
            {
                var questions = await _questionService.GetAll();

                if (questions == null)
                {
                    return NotFound();
                }

                return Ok(questions);
            }catch(QuestionNotFoundException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _questionService.DeleteQuestion(id);

                return Ok("Question deleted successfully!");
            }catch(QuestionNotFoundException ex)
            {
                _logger.LogInformation(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Test")]
        [AllowAnonymous]
        public async Task<IActionResult> Test(string url)
        {
            var client = new HttpClient();

            var result = await client.GetAsync(new Uri(url));

            return Ok($"{await result.Content.ReadAsStreamAsync()}\n\n{result.Content.Headers.ContentType.MediaType}");
        }

        [HttpPost("GenerateXML")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateXMl(List<QuestionCreateDTO> questions)
        {
            var result = ToXML(questions);
            return Ok(result);
        }

        [HttpPost("UploadQuestionImage")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadQuestionImage(IFormFile? image, string? imageUrl)
        {
            if(image == null && imageUrl != null)
            {
                var fN = Guid.NewGuid().ToString();
                var uploadPicture = await UploadToBlob(null, imageUrl, fN);

                var img = $"{_configuration.GetValue<string>("BlobConfig:CDNLife")}{fN}.jpg";

                return Ok(img);
            }else if(image != null && imageUrl == null)
            {
                var uploadPicture = await UploadToBlob(image, null, null);

                var img = $"{_configuration.GetValue<string>("BlobConfig:CDNLife")}{image.FileName + Path.GetExtension(image.FileName)}";

                return Ok(imageUrl);
            }

            return BadRequest();
        }

        [NonAction]
        public async Task<PutObjectResponse> UploadToBlob(IFormFile? image, string? imageUrl, string? fN)
        {
            string serviceURL = _configuration.GetValue<string>("BlobConfig:serviceURL");
            string AWS_accessKey = _configuration.GetValue<string>("BlobConfig:accessKey");
            string AWS_secretKey = _configuration.GetValue<string>("BlobConfig:secretKey");
            var bucketName = _configuration.GetValue<string>("BlobConfig:bucketName");
            var keyName = _configuration.GetValue<string>("BlobConfig:defaultFolder");

            var config = new AmazonS3Config() { ServiceURL = serviceURL };
            var s3Client = new AmazonS3Client(AWS_accessKey, AWS_secretKey, config);

            var request = new PutObjectRequest();

            if (image != null && imageUrl == null)
            {
                keyName = String.Concat(keyName, image.FileName);

                var fs = image.OpenReadStream();
                request.BucketName = bucketName;
                request.Key = keyName;
                request.InputStream = fs;
                request.ContentType = image.ContentType;
                request.CannedACL = S3CannedACL.PublicRead;
            }
            else if (image == null && imageUrl != null)
            {
                using (var client = new HttpClient())
                {
                    var result = await client.GetAsync(new Uri(imageUrl));
                    keyName = String.Concat(keyName, $"{fN}.jpg");

                    var fs = await result.Content.ReadAsStreamAsync();
                    request.BucketName = bucketName;
                    request.Key = keyName;
                    request.InputStream = fs;
                    request.ContentType = result.Content.Headers.ContentType.MediaType;
                    request.CannedACL = S3CannedACL.PublicRead;
                }
            }

            return await s3Client.PutObjectAsync(request);
        }

        [NonAction]
        public static string ToXML(List<QuestionCreateDTO> question)
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(question.GetType());
                serializer.Serialize(stringwriter, question);
                return stringwriter.ToString();
            }
        }
    }
}
