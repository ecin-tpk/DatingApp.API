using System.Threading.Tasks;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    public class LikesController : BaseController
    {
        private readonly ILikeService _likeService;


        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        // POST: Like a user
        [HttpPost("{recipientId}")]
        public async Task<IActionResult> LikeUser(int userId, int recipientId)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _likeService.LikeUser(userId, recipientId);

            return Ok();
        }
    }
}
