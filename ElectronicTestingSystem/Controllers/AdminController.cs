using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("api/admin")]
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [Route("addRoles")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var role = await _roleManager.RoleExistsAsync(roleName);

            if (!role)
            {
                var CreateRole = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (CreateRole.Succeeded)
                {
                    return Ok($"Role {roleName} created successfully!");
                }
                else
                {
                    return BadRequest($"Role {roleName} could not be created!");
                }
            }

            return BadRequest("Role already exists!");
        }
    }
}
