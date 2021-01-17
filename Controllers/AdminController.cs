using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Helpers.RequestParams;
using DatingApp.API.Models.Account;
using DatingApp.API.Models.Admin;
using DatingApp.API.Models.Reports;
using DatingApp.API.Services;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : BaseController
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IReportService _reportService;

        public AdminController(
            DataContext context,
            IMapper mapper,
            IAccountService accountService,
            IUserService userService,
            IPhotoService photoService,
            IReportService reportService)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
            _userService = userService;
            _photoService = photoService;
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

        #region Users
        // GET: Get all users for admin (paginated)
        [HttpGet("users/count-users/{year:int}")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> CountUsers(int year)
        {
            return Ok(await _userService.CountUsers(year));
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

        // GET: Get full details of a user
        [HttpGet("users/{id:int}")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserDetails(id);

            if (User.Id == id)
            {
                return Ok(_mapper.Map<LoginResponse>(user));
            }

            var test = _mapper.Map<UserDetailsForAdminResponse>(user);

            //var activityIds = _context.Interests.Where(i => i.UserId == test.Id).Select(i => i.ActivityId);
            //test.Interests = _context.Activities.Where(a => activityIds.Contains(a.Id)).Select(a => new InterestResponse { Id = a.Id, Label = a.Label }).ToList();

            //return Ok(_mapper.Map<UserDetailsResponse>(user));
            return Ok(test);

            //userParams.UserId = User.Id;
            //userParams.ForAdmin = true;

            //var users = await _userService.GetPagination(userParams);

            //Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            //return Ok(_mapper.Map<IEnumerable<UserForAdminResponse>>(users));
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
        public async Task<IActionResult> CountUsersByStatus()
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
            var usersByAge = await _userService.CountUsersByAge(year);

            return Ok(usersByAge);
        }
        #endregion

        #region Reports
        // GET: Get user reports (paginated)
        [HttpGet("reports/pagination")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> GetUserReports([FromQuery] ReportParams reportParams)
        {
            var reports = await _reportService.GetPagination(reportParams);
            var mappedReports = _mapper.Map<IEnumerable<ReportResponse>>(reports);
            foreach (var report in mappedReports)
            {
                report.UserName = await _context.Users.Where(u => u.Id == report.UserId).Select(u => u.Name).FirstOrDefaultAsync();
                report.PhotoUrl = await _context.Photos.Where(p => p.UserId == report.UserId && p.Order == 0).Select(p => p.Url).FirstOrDefaultAsync();
                report.SenderName = await _context.Users.Where(u => u.Id == report.SenderId).Select(u => u.Name).FirstOrDefaultAsync();
                report.ApprovedCount = (byte)await _context.Reports.Where(r => r.UserId == report.UserId && r.Status == ReportStatus.Approved).CountAsync();
            }

            Response.AddPagination(reports.CurrentPage, reports.PageSize, reports.TotalCount, reports.TotalPages);

            return Ok(mappedReports);
        }

        // PUT: Approve of disapprove report
        [HttpPut("reports/{id:int}")]
        public async Task<IActionResult> UpdateReportStatus(int id, UpdateStatusRequest model)
        {
            await _reportService.UpdateStatus(id, model);

            return Ok(new { message = "Report updated successfully" });
        }

        // GET: Count reports by status
        [HttpGet("reports/count")]
        public async Task<IActionResult> CountReportsByStatus()
        {
            var counts = await _reportService.CountByStatus();

            return Ok(counts);
        }
        #endregion

        // GET: Count number of uploaded photos
        [HttpGet("photos/count-photos/{milliseconds}")]
        [Authorize(Role.Admin)]
        public async Task<IActionResult> CountPhotos(double milliseconds)
        {
            return Ok(await _photoService.CountPhotos(milliseconds));
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
