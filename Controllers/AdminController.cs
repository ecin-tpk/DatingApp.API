using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Users;
using DatingApp.API.Services;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController: BaseController
    {
        private readonly IMapper _mapper;

        private readonly IAccountService _accountService;

        private readonly IUserService _userService;

        public AdminController(IMapper mapper, IAccountService accountService, IUserService userService)
        {
            _mapper = mapper;

            _accountService = accountService;

            _userService = userService;
        }

        // POST: Login for admin
        [HttpPost("account/login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            model.Role = Role.Admin;

            var dd = new DeviceDetector(Request.Headers["User-Agent"]);

            var response = await _accountService.Login(model, IpAddress(), dd);

            Response.SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        // GET: Get all users for admin (paginated)
        [HttpGet("users")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetPagination([FromQuery] UserParams userParams)
        {
            userParams.UserId = User.Id;

            var users = await _userService.GetPagination(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(_mapper.Map<IEnumerable<UserResponse>>(users));
        }

        // GET: Get number of users by status
        [HttpGet("users/status")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetNumberOfUsersByStatus()
        {
            var countByStatus = await _userService.GetNumberOfUsersByStatus();

            return Ok(countByStatus);
        }

        // Get IP address
        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwared-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
