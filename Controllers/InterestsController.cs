using AutoMapper;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Models.Interests;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    public class InterestsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IInterestService _interestService;

        public InterestsController(IMapper mapper,IInterestService interestService)
        {
            _mapper = mapper;
            _interestService = interestService;
        }

        // GET: Get all interests

        // GET: Get interests of user
        [HttpGet]
        public IActionResult GetAll(int userId)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var interests = _interestService.GetAll(userId);

            return Ok(_mapper.Map<IEnumerable<InterestResponse>>(interests));
        }

        // POST: Add an interest
        [HttpPost("{activityId}")]
        public async Task<IActionResult> Add(int userId, int activityId)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _interestService.AddInterest(userId, activityId);

            return Ok();
        }

        // POST: Remove an interest
        [HttpDelete("{activityId}")]
        public async Task<IActionResult> Remove(int userId, int activityId)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _interestService.RemoveInterest(userId, activityId);

            return Ok();
        }
    }
}
