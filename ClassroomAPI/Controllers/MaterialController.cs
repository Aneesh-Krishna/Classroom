using ClassroomAPI.Data;
using ClassroomAPI.Models;
using ClassroomAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly FileService _fileService;
        public MaterialController(ClassroomDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        //Get all materials of a course
        [HttpGet("{courseId}/GetMaterials")]
        public async Task<IActionResult> GetAllMaterials(Guid courseId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if(course == null)
                return NotFound("Course not found!");

            var isCourseMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == courseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;

            if (!isCourseMember)
                return Unauthorized("You're not authorized!");

            var materials = await _context.Materials
                .Where(m => m.CourseId == courseId)
                .Select(m => new
                {
                    m.MaterialId,
                    m.MaterialName,
                    m.MaterialUrl
                })
                .ToListAsync();

            return Ok(materials);
        }

        //Get a material by id
        [HttpGet("{materialId}/GetMaterial")]
        public async Task<IActionResult> GetMaterialById(Guid materialId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == materialId);
            if (material == null)
                return NotFound("Material not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == material.CourseId);
            if(course == null) 
                return NotFound("Course not found!");

            var isCourseMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isCourseMember)
                return Unauthorized("You're not authorized!");

            return Ok(material);
        }

        //Upload material
        [HttpPost("{courseId}/UploadMaterial")]
        public async Task<IActionResult> UploadMaterial(Guid courseId,  IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            if (file == null)
                return BadRequest("Upload a file!");

            var fileUrl = await _fileService.UploadFileAsync(file);
            
            var material = new Material
            {
                MaterialId = Guid.NewGuid(),
                MaterialName = file.FileName,
                MaterialUrl = fileUrl,
                CourseId = courseId,
                Course = course
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return Ok(material);
        }

        //Delete a material
        [HttpDelete("{materialId}/DeleteMaterial")]
        public async Task<IActionResult> DeleteMaterial(Guid materialId)
        {
            var userId = GetCurrentUserId();
            if(userId == null)
                return Unauthorized("User Id not found!");
            
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == materialId);
            if (material == null)
                return NotFound("Material not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == material.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return Ok(material.MaterialName +" has been deleted!");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            var fileUrl = await _fileService.UploadFileAsync(file);
            return Ok(new { Url = fileUrl });
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
