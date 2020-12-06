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

        // GET: Get users (paginated)
        [HttpGet("pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] UserParams userParams)
        {
            userParams.UserId = User.Id;
            userParams.Name = null;
            userParams.OrderBy = null;
            userParams.Status = Status.Active;

            var users = await _userService.GetPagination(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            if (userParams.IsMatched || userParams.Likers || userParams.TopPicks)
            {
                return Ok(_mapper.Map<IEnumerable<SimpleUserResponse>>(users));
            }


            return Ok(_mapper.Map<IEnumerable<UserResponse>>(users));
        }

        // GET: Get a specific user by id
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var user = await _userService.GetById(id);

            if (User.Id == id)
            {
                return Ok(_mapper.Map<LoginResponse>(user));
            }

            return Ok(_mapper.Map<UserDetailsResponse>(user));
        }

        // PUT: Update user details
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponse>> Update(int id, [FromBody]UpdateRequest model)
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
