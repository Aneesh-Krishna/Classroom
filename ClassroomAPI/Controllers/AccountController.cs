using ClassroomAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClassroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        //Register a new admin
        [Authorize(Roles = "Admin")]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.userName,
                PhoneNumber = model.phoneNumber,
                Email = model.email,
                FullName = model.fullName,
                Role = Roles.Admin
            };

            var result = await _userManager.CreateAsync(user, model.password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, model.role);
            return Ok(new { user.UserName });
        }

        //Registration endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.userName,
                PhoneNumber = model.phoneNumber,
                Email = model.email,
                FullName  = model.fullName,
                Role = Roles.User
            };

            var result = await _userManager.CreateAsync(user, model.password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, model.role);
            return Ok(new { user.UserName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.userName);
            if (user == null)
                return BadRequest("Username not found!");

            var result = await _signInManager.PasswordSignInAsync(user, model.password, false, false);
            if (!result.Succeeded)
                return BadRequest("Wrong username or password!");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        public string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim> 
            { 
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(ClaimTypes.Role, user.Role.ToString()) 
            }; 
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); 
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); 
            var token = new JwtSecurityToken
                (
                    issuer: _configuration["Jwt:Issuer"], 
                    audience: _configuration["Jwt:Audience"], 
                    claims: claims, 
                    expires: DateTime.Now.AddHours(1), 
                    signingCredentials: creds
                ); 
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    //Models for registration and login
    public class RegisterModel
    { 
        public string userName { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        [EmailAddress]
        public string email { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string phoneNumber {  get; set; } = string.Empty;
        public string role { get; set; } = "User";
    }

    public class LoginModel
    {
        public string userName { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}
