using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Models.Photos;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/photos")]
    [Authorize]
    public class PhotosController : BaseController
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        // GET: Get photo by id
        [HttpGet("{id}", Name = "GetPhotoById")]
        public async Task<IActionResult> GetById(int id)
        {
            var photo = await _photoService.GetById(id);

            return Ok(photo);
        }

        // POST: Upload photo
        [HttpPost]
        public async Task<IActionResult> Upload(int userId, [FromForm] UploadRequest model)
        {
            // Users can upload their own photo and admins can update any user's photo
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var photo = await _photoService.Upload(userId, model);

            return CreatedAtRoute("GetPhotoById", new { userId, id = photo.Id }, photo);
        }

        // POST: Save photo url
        [HttpPost]
        public async Task<IActionResult> Create(int userId, [FromForm] UploadRequest model)
        {
            // Users can upload their own photo and admins can update any user's photo
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var photo = await _photoService.Upload(userId, model);

            return CreatedAtRoute("GetPhotoById", new { userId, id = photo.Id }, photo);
        }

        // POST: Set as main photo
        [HttpPost("{id}/set-main")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            // Users can upload their own photo and admins can update any user's photo
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _photoService.SetMain(userId, id);

            return NoContent();
        }

        // DELETE: Delete photo
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            // Users can delete their own photo and admins can delete any user's photo
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _photoService.Delete(userId, id);
            return Ok();
        }
    }
}
