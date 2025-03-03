using ClassroomAPI.Data;
using ClassroomAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ClassroomDbContext _context;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, ClassroomDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [Authorize]
        [HttpGet("userName")]
        public async Task<IActionResult> MyUsername()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userName = await _context.Users.Where(u => u.Id == userId).Select(u => u.UserName).SingleOrDefaultAsync();
            
            return Ok(userName); 
        }

        [Authorize]
        [HttpGet("isAdmin")]
        public async Task<IActionResult> isAdmin()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found!");

            if (user.Role != Roles.Admin)
                return BadRequest("Not the admin");

            return Ok("Admin");
        }

        [Authorize]
        [HttpGet("{searchUserName}/users")]
        public async Task<IActionResult> SearchedUsers(string searchUserName)
        {
            var myUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myUserId == null)
                return Unauthorized("You're not authorized!");

            var normalizedSearchName = searchUserName.ToUpper();

            var users = await _context.Users
                .Where(u => EF.Functions.Like(u.UserName.ToUpper(), $"%{normalizedSearchName}%"))
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber
                })
                .ToListAsync();

            if (users == null || !users.Any())
                return NotFound("No such user found!");

            return Ok(users);
        }
 
        //Get a user's details
        [Authorize]
        [HttpGet("{userId}/details")]
        public async Task<IActionResult> UserDetails(string userId)
        {
            var myUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myUserId == null) 
                return Unauthorized("You're not authorized!");

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber
                })
                .ToListAsync();

            if (user == null)
                return NotFound("User not found!");

            return Ok(user);
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

            await _userManager.AddToRoleAsync(user, user.Role.ToString());
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

            await _userManager.AddToRoleAsync(user, user.Role.ToString());
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

        private string GenerateJwtToken(ApplicationUser user)
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
    }

    public class LoginModel
    {
        public string userName { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}
