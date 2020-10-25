using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Models.Reports;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    [Authorize]
    public class ReportsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;

        public ReportsController(IMapper mapper, IReportService reportService)
        {
            _mapper = mapper;
            _reportService = reportService;
        }

        // POST: Send report
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, NewReportRequest model)
        {
            return Ok();
        }

        // DELETE: Delete report
        [HttpPost]
        public async Task<IActionResult> DeleteReport(int userId, NewReportRequest model)
        {
            return Ok();
        }
    }
}
