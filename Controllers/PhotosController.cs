using System.Threading.Tasks;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Models.Photos;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Http;
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
        [HttpGet("{id:int}", Name = "GetPhotoById")]
        public async Task<IActionResult> GetById(int id)
        {
            var photo = await _photoService.GetById(id);

            return Ok(photo);
        }

        // POST: Upload photo
        [HttpPost]
        public async Task<IActionResult> Upload(int userId, [FromForm] UploadRequest model)
        {
            if (model.File.Length == 0)
            {
                throw new AppException("The field Order is required");
            }

            // Users can upload their own photo and admins can update any user's photo
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var photo = await _photoService.Upload(userId, model);

            return CreatedAtRoute("GetPhotoById", new { userId, id = photo.Id }, photo);
        }

        // POST: Save photo url
        [HttpPost("url")]
        public async Task<IActionResult> SaveUrl(int userId, [FromBody] UploadRequest model)
        {
            if (model.Url == null)
            {
                throw new AppException("The field Url is required");
            }
            if (model.PublicId == null)
            {
                throw new AppException("The field PublicId is required");
            }
            // Users can upload their own photo and admins can update any user's photo
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var photo = await _photoService.SavePhotoUrl(userId, model);

            return CreatedAtRoute("GetPhotoById", new { userId, id = photo.Id }, photo);
        }

        // PUT: Change photo order
        [HttpPut("{id:int}/{order:int}")]
        public async Task<IActionResult> ChangeOrder(int userId, int photoId, byte order)
        {
            // Users can change their own data and admins can change any user's data
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _photoService.ChangeOrder(photoId, order);

            return Ok("Changed photo order successfully");
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
