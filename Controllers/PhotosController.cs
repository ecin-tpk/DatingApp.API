using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Dtos;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using DatingApp.API.Models.Photos;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        [HttpGet("{id}", Name = "GetById")]
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
            if (userId != User.Id && User.Role != Entities.Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var photo = await _photoService.Upload(userId, model);

            return CreatedAtRoute("GetById", new { userId, id = photo.Id }, photo);
        }

        [HttpPost("{id}/set-main")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            // Users can upload their own photo and admins can update any user's photo
            if (userId != User.Id && User.Role != Entities.Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _photoService.SetMain(userId, id);

            return NoContent();
        }

        //        [HttpDelete("{id}")]
        //        public async Task<IActionResult> DeletePhoto(int userId, int id)
        //        {
        //            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //            {
        //                return Unauthorized();
        //            }

        //            var user = await _repo.GetUser(userId);

        //            if (!user.Photos.Any(p => p.Id == id))
        //            {
        //                return Unauthorized();
        //            }

        //            var photoFromRepo = await _repo.GetPhoto(id);

        //            if (photoFromRepo.IsMain)
        //            {
        //                return BadRequest("You cannot delete your main photo");
        //            }

        //            if (photoFromRepo.PublicID != null)
        //            {
        //                var deleteParams = new DeletionParams(photoFromRepo.PublicID);

        //                var result = _cloudinary.Destroy(deleteParams);

        //                if (result.Result == "ok")
        //                {
        //                    _repo.Delete(photoFromRepo);
        //                }
        //            }

        //            if (photoFromRepo.PublicID == null)
        //            {
        //                _repo.Delete(photoFromRepo);
        //            }

        //            if (await _repo.SaveAll())
        //            {
        //                return Ok();
        //            }

        //            return BadRequest("Failed to delete the photo");
        //        }
    }
}
