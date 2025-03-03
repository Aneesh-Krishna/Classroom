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
    public class LibraryMaterialController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly FileService _fileService;

        public LibraryMaterialController(ClassroomDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        //Endpoint to get all library-materials
        [HttpGet]
        public async Task<IActionResult> GetAllLibraryMaterials()
        {
            var userId = GetCurrentUserID();
            if(userId == null)
            {
                return Unauthorized("User Id not found!");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null)
            {
                return NotFound("User not found!");
            }

            var libraryMaterials = await _context.LibraryMaterials
                .Select(lm => new
                {
                    lm.LibraryMaterialUploadId,
                    lm.LibraryMaterialUploadName,
                    lm.LibraryMaterialUploadUrl,
                    lm.UploaderId,
                    Uploader = lm.Uploader.FullName ?? ""
                })
                .ToListAsync();

            return Ok(libraryMaterials);
        }

        //Endpoint to get a specific library material
        [HttpGet("{libraryMaterialId}")]
        public async Task<IActionResult> GetMaterial(Guid libraryMaterialId)
        {
            var userId = GetCurrentUserID();
            if (userId == null)
            {
                return Unauthorized("User Id not found!");
            }


            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found!");
            }

            var libraryMaterial = await _context.LibraryMaterials.FirstOrDefaultAsync(lm => lm.LibraryMaterialUploadId == libraryMaterialId);
            if (libraryMaterial == null)
                return NotFound("Material not found");

            var returnLibraryMaterial = new
            {
                LibraryMaterialUploadId = libraryMaterial.LibraryMaterialUploadId,
                LibraryMaterialUploadName = libraryMaterial.LibraryMaterialUploadName,
                LibraryMaterialUploadUrl = libraryMaterial.LibraryMaterialUploadUrl,
                UploaderId = libraryMaterial.UploaderId,
                Uploader = libraryMaterial.Uploader.FullName ?? ""
            };

            return Ok(returnLibraryMaterial);
        }

        //Endpoint to get library-materials uploaded by a specific user
        [HttpGet("{uploaderId}")]
        public async Task<IActionResult> GetMaterialsByUser(string uploaderId)
        {
            var userId = GetCurrentUserID();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found!");
            }

            if(user.Role != Roles.Admin)
            {
                return Unauthorized("You're not authorized!");
            }

            var uploader = await _context.Users.FirstOrDefaultAsync(u => u.Id == uploaderId);
            if (uploader == null)
                return BadRequest("Uploader not found!");

            var libraryMaterials = await _context.LibraryMaterials
                .Where(lm => lm.UploaderId == uploaderId)
                .Select(lm => new
                {
                    lm.LibraryMaterialUploadId,
                    lm.LibraryMaterialUploadName,
                    lm.LibraryMaterialUploadUrl,
                    lm.UploaderId,
                    Uploader = lm.Uploader.FullName ?? ""
                })
                .ToListAsync();

            if (libraryMaterials == null)
                return NoContent();

            return Ok(libraryMaterials);
        }

        //Endpoint for Material Uploading
        [HttpPost("upload-library-material")]
        public async Task<IActionResult> UploadLibraryMaterial(IFormFile file)
        {
            var userId = GetCurrentUserID();

            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found!");

            var fileUrl = await _fileService.UploadFileAsync(file);

            var libraryMaterial = new LibraryMaterialUpload
            {
                LibraryMaterialUploadId = Guid.NewGuid(),
                LibraryMaterialUploadName = file.FileName,
                LibraryMaterialUploadUrl = fileUrl,
                UploaderId = userId,
                AcceptedOrRejected = "",
                Uploader = user
            };

            _context.LibraryMaterials.Add(libraryMaterial);
            await _context.SaveChangesAsync();

            var returnLibraryMaterial = new
            {
                LibraryMaterialUploadId = libraryMaterial.LibraryMaterialUploadId,
                LibraryMaterialUploadName = libraryMaterial.LibraryMaterialUploadName,
                LibraryMaterialUploadUrl = libraryMaterial.LibraryMaterialUploadUrl,
                UploaderId = libraryMaterial.UploaderId,
                Uploader = libraryMaterial.Uploader.FullName ?? ""
            };

            return Ok(returnLibraryMaterial);
        }

        //Endpoint for accepting the material(Only for application's admin)
        [HttpPut("{libraryMaterialId}/Accept")]
        public async Task<IActionResult> AcceptLibraryMaterial(Guid libraryMaterialId)
        {
            var userId = GetCurrentUserID();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found!");

            if (user.Role != Roles.Admin)
                return Unauthorized("You're not authorized!");

            var libraryMaterial = await _context.LibraryMaterials.FirstOrDefaultAsync(lm => lm.LibraryMaterialUploadId == libraryMaterialId);
            if (libraryMaterial == null)
                return NotFound("Material not found!");

            libraryMaterial.AcceptedOrRejected = "Accepted";

            await _context.SaveChangesAsync();

            var returnLibraryMaterial = new
            {
                LibraryMaterialUploadId = libraryMaterial.LibraryMaterialUploadId,
                LibraryMaterialUploadName = libraryMaterial.LibraryMaterialUploadName,
                LibraryMaterialUploadUrl = libraryMaterial.LibraryMaterialUploadUrl,
                UploaderId = libraryMaterial.UploaderId,
                Uploader = libraryMaterial.Uploader.FullName ?? ""
            };

            return Ok(returnLibraryMaterial);
        }

        //Endpoint for rejecting the material(Only for application's admin)
        [HttpDelete("{libraryMaterialId}/Reject")]
        public async Task<IActionResult> RejectLibraryMaterial(Guid libraryMaterialId)
        {
            var userId = GetCurrentUserID();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found!");

            if (user.Role != Roles.Admin)
                return Unauthorized("You're not authorized!");

            var libraryMaterial = await _context.LibraryMaterials.FirstOrDefaultAsync(lm => lm.LibraryMaterialUploadId == libraryMaterialId);
            if (libraryMaterial == null)
                return NotFound("Material not found!");

            //libraryMaterial.AcceptedOrRejected = "Rejected";

            _context.LibraryMaterials.Remove(libraryMaterial);
            await _context.SaveChangesAsync();

            return Ok("Material has been rejected!");
        }

        //Method for uploading the material
        public async Task<IActionResult> UploadMaterial(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            var fileUrl = await _fileService.UploadFileAsync(file);
            return Ok(new { Url = fileUrl });
        }

        private string GetCurrentUserID()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
