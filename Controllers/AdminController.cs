using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Helpers.RequestParams;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Admin;
using DatingApp.API.Services;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IReportService _reportService;

        public AdminController(IMapper mapper, IAccountService accountService, IUserService userService, IReportService reportService)
        {
            _mapper = mapper;
            _accountService = accountService;
            _userService = userService;
            _reportService = reportService;
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
            userParams.ForAdmin = true;

            var users = await _userService.GetPagination(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(_mapper.Map<IEnumerable<UserForAdminResponse>>(users));
        }

        // GET: Create user
        [HttpPost("users")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> Create(NewUserRequest model)
        {
            var user = await _userService.Create(model);

            return Ok(user);
        }

        // GET: Get number of users by status
        [HttpGet("users/status-count")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetNumberOfUsersByStatus()
        {
            var countByStatus = await _userService.GetNumberOfUsersByStatus();

            return Ok(countByStatus);
        }

        // GET: Get number of new users per month
        [HttpGet("users/new-users-per-month/{year:int}")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetNewUsersPerMonth(int year)
        {
            var newUsersPerMonth = await _userService.GetNewUsersPerMonth(year);

            return Ok(newUsersPerMonth);
        }

        // GET: Get total users
        [HttpGet("users/users-per-month/{year:int}")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetTotalUsersPerMonth(int year)
        {
            var usersPerMonth = await _userService.GetTotalUsersPerMonth(year);

            return Ok(usersPerMonth);
        }

        // GET: Percentage users by age
        [HttpGet("users/users-by-age/{year:int}")]
        public async Task<IActionResult> GetUsersByAge(int year)
        {
            var usersByAge = await _userService.GetUsersByAge(year);

            return Ok(usersByAge);
        }

        // GET: Get user reports (paginated)
        [HttpGet("users/reports")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetUserReports([FromQuery] ReportParams reportParams)
        {
            var reports = await _reportService.GetPagination(reportParams);

            Response.AddPagination(reports.CurrentPage, reports.PageSize, reports.TotalCount, reports.TotalPages);

            return Ok(reports);
        }

        // DELETE: Delete user report
        [HttpDelete("users/reports/{id:int}")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> DeleteReport(int id)
        {
            await _reportService.Delete(id);

            return Ok("Report deleted successfully");
        }

        //// POST: Create new interest subject
        //[HttpPost("/interests")]
        //[Authorize(Role.Admin)]
        //public async Task<IActionResult> CreateInterestSubject(int id)
        //{
        //    await _interestService.Create(id);

        //    return Ok("Report deleted successfully");
        //}

        // Helpers
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
