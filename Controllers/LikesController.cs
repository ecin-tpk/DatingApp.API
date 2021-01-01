using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Hubs;
using DatingApp.API.Models.Users;
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
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILikeService _likeService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public LikesController(IMapper mapper, IUserService userService, ILikeService likeService, IHubContext<NotificationHub> notificationHub)
        {
            _mapper = mapper;
            _likeService = likeService;
            _userService = userService;
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

            if (userId == recipientId)
            {
                throw new AppException("I love myself too");
            }

            if(await _likeService.LikeUser(userId, recipientId, super))
            {
                await _notificationHub.Clients
                    .User(recipientId.ToString())
                    .SendAsync("receiveMatchNotification", await _userService.GetSimpleUser(userId));

                var test = _likeService.GetMatchUserWithFcmTokens(recipientId);

                //return Ok(await _userService.GetSimpleUser(recipientId));                
                
                return Ok(await test);
            }   

            return Ok();
        }

        // PATCH: 
        [HttpPatch("{recipientId:int}/unmatch")]
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
