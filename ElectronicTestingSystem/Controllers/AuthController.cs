using ElectronicTestingSystem.Helpers;
using ElectronicTestingSystem.Models.DTOs;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ElectronicTestingSystem.Controllers
{
    [ApiController]
    [Route("api/auth/users")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserService _userService;
        private readonly string _jwtConfiguration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;

            _jwtConfiguration = configuration.GetValue<string>("JWTConfig:Secret");
            _userService = userService;
        } 

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userToCreate)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByNameAsync(userToCreate.UserName);

                if (existingUser == null)
                {
                    var newUser = new IdentityUser()
                    {
                        Email = userToCreate.Email,
                        UserName = userToCreate.UserName,
                        PhoneNumber = userToCreate.PhoneNumber

                    };

                    var Created = await _userManager.CreateAsync(newUser, userToCreate.Password);

                    if (Created.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, "User");

                        User user = new User
                        {
                            Id = newUser.Id,
                            FirstName = userToCreate.FirstName,
                            LastName = userToCreate.LastName,
                            Gender = userToCreate.Gender,
                            Email = userToCreate.Email,
                            UserName = userToCreate.UserName,
                            PhoneNumber = userToCreate.PhoneNumber,
                            DateOfBirth = userToCreate.DateOfBirth

                        };

                        var existingUserCustom = await _userService.GetUser(user.Id);

                        if(existingUserCustom == null)
                        {
                            await _userService.CreateUser(user);
                        }

                        var jwtToken = GenerateJWT(newUser);
                        return Ok(new AuthResult
                        {
                            Token = jwtToken,
                            Succedded = true,
                        });
                    }
                    else
                    {
                        return BadRequest(new AuthResult()
                        {
                            Errors = Created.Errors.Select(error => error.Description).ToList(),
                            Succedded = false
                        }); ;
                    }
                }
                else
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>
                        {
                            "User Already Exists"
                        },
                        Succedded = false
                    });
                }

            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>
                {
                    "Invalid Request"
                },
                Succedded = false
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLogInDTO user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser == null) // Check if user already exists
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>
                        {
                            "This user does not exist!"
                        },
                        Succedded = false
                    });
                }

                var validUser = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if (!validUser)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>
                        {
                            "Incorrect Password!"
                        },
                        Succedded = false
                    });
                }

                var jwtToken = GenerateJWT(existingUser);

                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                    Succedded = true
                });
            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>
                {
                    "Invalid Request"
                },
                Succedded = false
            });
        }

        private string GenerateJWT(IdentityUser user)
        {            
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfiguration);

            var claims = GetAllClaims(user);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Result),
                Expires = DateTime.Now.AddHours(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Issuer = "http://localhost:42654",
                Audience = "http://localhost:44383"
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        private async Task<List<Claim>> GetAllClaims(IdentityUser user)
        {
            var _options = new IdentityOptions();

            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(userRole);

                if(role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));

                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach(var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }

            return claims;
        }
    }
}