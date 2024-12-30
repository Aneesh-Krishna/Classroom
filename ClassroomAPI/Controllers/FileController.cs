using ClassroomAPI.Data;
using ClassroomAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly FileService _fileService;

        public FileController(ClassroomDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest("User ID not found.");

            var fileStream = await _fileService.DownloadFileAsync(fileName);
            if (fileStream == null)
                return NotFound("File not found.");

            // Return the file stream with appropriate headers
            return File(fileStream, "application/octet-stream", fileName);
        }
    }
}
