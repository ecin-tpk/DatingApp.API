using System.Threading.Tasks;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Hubs;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    public class LikesController : BaseController
    {
        private readonly ILikeService _likeService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public LikesController(ILikeService likeService, IHubContext<NotificationHub> notificationHub)
        {
            _likeService = likeService;
            _notificationHub = notificationHub;
        }

        // POST: Like a user
        [HttpPost("{recipientId}")]
        public async Task<IActionResult> LikeUser(int userId, int recipientId, [FromQuery] bool super)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _likeService.LikeUser(userId, recipientId, super);

            return Ok();
        }

        // PATCH: 
        [HttpPatch("")]
        public async Task<IActionResult> Unmatch(int userId, int recipientId)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _likeService.Unmatch(userId, recipientId);

            return Ok();
        }
    }
}
