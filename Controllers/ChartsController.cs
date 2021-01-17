using DatingApp.API.Entities;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Role.Admin)]
    public class ChartsController : BaseController
    {
        private readonly IChartService _chartService;
        public ChartsController(IChartService chartService)
        {
            _chartService = chartService;
        }

        // GET: New users per month last 12 months
        [HttpGet("users/new-users-per-month/{milliseconds}")]
        public async Task<IActionResult> GetNewUsersPerMonth(double milliseconds)
        {
            return Ok(await _chartService.GetNewUsersPerMonth(milliseconds));
        }

        // GET: Total users per specific period
        [HttpGet("users/total-users/{milliseconds}/{period}")]
        public async Task<IActionResult> GetTotalUsers(double milliseconds, string period)
        {
            return Ok(await _chartService.GetTotalUsers(milliseconds, period));
        }

        // GET: New active percentage of user
        [HttpGet("users/active-percentage/{milliseconds}")]
        public async Task<IActionResult> GetActivePercentage(double milliseconds)
        {
            return Ok(await _chartService.GetActivePercentage(milliseconds));
        }
    }
}
