using ElectronicTestingSystem.Models.DTOs;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IUserService userService, UserManager<IdentityUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
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

            var identityUser = await _userManager.FindByIdAsync(user.Id);
            identityUser.UserName = user.UserName;
            identityUser.PhoneNumber = user.PhoneNumber;

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(User.Claims.Where(x => x.Type == "Id").FirstOrDefault()?.Value);

            if(user == null)
            {
                return BadRequest("User not found!");
            }

            await _userManager.DeleteAsync(user);
            
            await _userService.DeleteUser(id);

            return Ok("User has been deleted!");
        }
    }
}
