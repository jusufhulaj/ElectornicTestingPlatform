using ElectronicTestingSystem.Models.DTOs;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
              _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await _userService.GetUser(id);

            if(user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var users = await _userService.GetAllUsers();

            if(users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<User>> Update(string id, UserUpdateDTO userToUpdate)
        {
            var user = await _userService.UpdateUser(id, userToUpdate);

            if(user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
}
