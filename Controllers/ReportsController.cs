using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Hubs;
using DatingApp.API.Models.Reports;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    //[Authorize]
    public class ReportsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IHubContext<ReportsHub, IReportClient> _reportsHub;

        public ReportsController(IMapper mapper, IReportService reportService, IHubContext<ReportsHub, IReportClient> reportsHub)
        {
            _mapper = mapper;
            _reportService = reportService;
            _reportsHub = reportsHub;
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

            await _reportService.Create(userId, model);

            await _reportsHub.Clients.All.ReceiveReport(model);

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
