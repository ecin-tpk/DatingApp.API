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
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IUserService _userService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ReportsController(IMapper mapper, IReportService reportService, IUserService userService, IHubContext<NotificationHub> notificationHub)
        {
            _mapper = mapper;
            _reportService = reportService;
            _userService = userService;
            _notificationHub = notificationHub;
        }

        //// GET: Get message by id
        //[HttpGet("{id}", Name = "GetReportById")]
        //public async Task<IActionResult> GetById(int userId, int id)
        //{
        //    var message = await _reportService.GetById(id);
        //    if (message == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(message);
        //}

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

        // DELETE: Delete report
        //[HttpDelete]
        //public async Task<IActionResult> DeleteReport(int userId, NewReportRequest model)
        //{
        //    return Ok();
        //}
    }
}
