using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Users;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IMapper _mapper;

        private readonly IUserService _userService;

        public UsersController(IMapper mapper, IUserService userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        //// GET: Get all users for normal user (paginated)
        //[HttpGet]
        //public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        //{
        //    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        //    var userFromRepo = await _repo.GetUser(currentUserId);

        //    userParams.UserId = currentUserId;

        //    // Prevent normal user search by name
        //    if ((!string.IsNullOrEmpty(userParams.Name) && userFromRepo.Role != "admin"))
        //    {
        //        userParams.Name = null;
        //    }

        //    // Prevent normal user search by verification
        //    if (userParams.Verification != null && userFromRepo.Role != "admin")
        //    {
        //        userParams.Verification = null;
        //    }

        //    // Prevent normal user search by status
        //    if ((!string.IsNullOrEmpty(userParams.Status) && userFromRepo.Role != "admin"))
        //    {
        //        userParams.Status = null;
        //    }

        //    if (string.IsNullOrEmpty(userParams.Gender))
        //    {
        //        userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
        //    }

        //    var users = await _repo.GetUsers(userParams);

        //    var usersToReturn = _mapper.Map<IEnumerable<UserForDetailedDto>>(users);

        //    Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        //    return Ok(usersToReturn);
        //}

        //// GET: Get all users for admin
        //[HttpGet("admin")]
        //[Authorize(Roles = "admin")]
        //public async Task<IActionResult> GetUsersForAdmin([FromQuery] UserParams userParams)
        //{
        //    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        //    userParams.UserId = currentUserId;

        //    if (string.IsNullOrEmpty(userParams.Gender))
        //    {
        //        userParams.Gender = "any";
        //    }

        //    var users = await _repo.GetUsers(userParams);

        //    var usersToReturn = _mapper.Map<IEnumerable<UserForAdmin>>(users);

        //    Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

        //    return Ok(usersToReturn);
        //}



        // GET: Get a specific user by id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            if(id!= User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var user = await _userService.GetById(id);

            return Ok(user);
        }

        // PUT: Update user details
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponse>> Update(int id, UpdateRequest model)
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

        //// POST: Like a user
        //[HttpPost("{id}/like/{recipientId}")]
        //public async Task<IActionResult> LikeUser(int id, int recipientId)
        //{
        //    if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //    {
        //        return Unauthorized();
        //    }

        //    var like = await _repo.GetLike(id, recipientId);

        //    if (like != null)
        //    {
        //        return BadRequest("You already like this user");
        //    }

        //    if (await _repo.GetUser(recipientId) == null)
        //    {
        //        return NotFound();
        //    }

        //    like = new Like
        //    {
        //        LikerId = id,
        //        LikeeId = recipientId
        //    };

        //    _repo.Add<Like>(like);

        //    if (await _repo.SaveAll())
        //    {
        //        return Ok();
        //    }

        //    return BadRequest("Failed to like user");
        //}
    }
}
