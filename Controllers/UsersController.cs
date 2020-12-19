using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.RequestParams;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Models.Users;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Models.Account;
using System.Linq;
using DatingApp.API.Models.Interests;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UsersController(DataContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        // GET: Get users (paginated)
        [HttpGet("pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] UserParams userParams)
        {
            // TODO: Check toppicks

            userParams.UserId = User.Id;
            userParams.Name = null;
            userParams.OrderBy = null;
            userParams.Status = Status.Active;
            userParams.ForAdmin = false;

            var users = await _userService.GetPagination(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            foreach (var user in users)
            {
                // ToList works
                user.Photos = user.Photos.OrderBy(p => p.Order).ToList();
            }

            if (userParams.IsMatched || userParams.Likers || userParams.TopPicks)
            {
                return Ok(_mapper.Map<IEnumerable<SimpleUserResponse>>(users));
            }

            var mappedUsers = _mapper.Map<IEnumerable<UserResponse>>(users);

            foreach (var user in mappedUsers)
            {
                var activityIds = _context.Interests.Where(i => i.UserId == user.Id).Select(i => i.ActivityId);
                user.Interests = _context.Activities.Where(a => activityIds.Contains(a.Id)).Select(a => new InterestResponse { Id = a.Id, Label = a.Label }).ToList();
            }

            //return Ok(_mapper.Map<IEnumerable<UserResponse>>(users));
            return Ok(mappedUsers);
        }

        // GET: Get a specific user by id
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetUserDetails(int id)
        {
            var user = await _userService.GetUserDetails(id);

            if (User.Id == id)
            {
                return Ok(_mapper.Map<LoginResponse>(user));
            }

            var test = _mapper.Map<UserDetailsResponse>(user);

            var activityIds = _context.Interests.Where(i => i.UserId == test.Id).Select(i => i.ActivityId);
            test.Interests = _context.Activities.Where(a => activityIds.Contains(a.Id)).Select(a => new InterestResponse { Id = a.Id, Label = a.Label }).ToList();

            //return Ok(_mapper.Map<UserDetailsResponse>(user));
            return Ok(test);
        }

        // PUT: Update user details
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UpdateRequest model)
        {
            // Users can update their own data and admins can update any user's data
            if (id != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            // Only admins can update role
            if (User.Role != Role.Admin)
            {
                model.Role = null;
            }

            var userToReturn = await _userService.Update(id, model);

            return Ok(userToReturn);
        }
    }
}
