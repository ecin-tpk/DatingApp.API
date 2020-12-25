using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Hubs;
using DatingApp.API.Models.Reports;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    [Authorize]
    public class ReportsController : BaseController
    {
        private readonly IReportService _reportService;
        private readonly IUserService _userService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ReportsController(IMapper mapper, IReportService reportService, IUserService userService, IHubContext<NotificationHub> notificationHub)
        {
            _reportService = reportService;
            _userService = userService;
            _notificationHub = notificationHub;
        }

        // POST: Send report
        [HttpPost]
        public async Task<IActionResult> CreateReport(int userId, NewReportRequest model)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var report = await _reportService.Create(userId, model);

            foreach (var id in _userService.GetAdminIds())
            {
                await _notificationHub.Clients.User(id.ToString()).SendAsync("ReceiveNotification", report);
            }

            return Ok("User reported successfully");
        }
    }
}
