using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.RequestParams;
using DatingApp.API.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IReportService
    {
        Task<PagedList<Report>> GetPagination(ReportParams reportParams);
        Task<Report> GetById(int id);
        Task Create(int userId, NewReportRequest model);
        Task Delete(int id);
    }
    #endregion

    public class ReportService : IReportService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ReportService(DataContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        // Get paginated reports
        public async Task<PagedList<Report>> GetPagination(ReportParams reportParams)
        {
            var reports = _context.Reports.AsQueryable();

            return await PagedList<Report>.CreateAsync(reports, reportParams.PageNumber, reportParams.PageSize);
        }

        // Create report
        public async Task Create(int userId, NewReportRequest model)
        {
            model.SenderId = userId;

            if (await _userService.GetUser(model.UserId) == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var report = _mapper.Map<Report>(model);

            _context.Add(report);

            await _context.SaveChangesAsync();
        }

        // Delete report
        public async Task Delete(int id)
        {
            var reportInDb = await _context.Reports.SingleOrDefaultAsync(r => r.Id == id);

            _context.Remove(reportInDb);

            await _context.SaveChangesAsync();
        }

        // Get report by id
        public Task<Report> GetById(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
