using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<Report> Create(int userId, NewReportRequest model);
        Task<int[]> CountByStatus();
        Task UpdateStatus(int id, UpdateStatusRequest model);
        Task Delete(int id);

        Task<IEnumerable<ReportResponse>> GetPaginationTest(ReportParams reportParams);
    }
    #endregion

    public class ReportService : IReportService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ReportService(DataContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReportResponse>> GetPaginationTest(ReportParams reportParams)
        {
            var reports = GetPagination(reportParams);
            var mappedReports = _mapper.Map<IEnumerable<ReportResponse>>(reports);
            foreach (var report in mappedReports)
            {
                report.UserName = await _context.Users.Where(u => u.Id == report.UserId).Select(u => u.Name).FirstOrDefaultAsync();
                report.PhotoUrl = await _context.Photos.Where(p => p.UserId == report.UserId && p.Order == 0).Select(p => p.Url).FirstOrDefaultAsync();
                report.SenderName = await _context.Users.Where(u => u.Id == report.SenderId).Select(u => u.Name).FirstOrDefaultAsync();
            }

            return null;
        }

        // Get paginated reports
        public async Task<PagedList<Report>> GetPagination(ReportParams reportParams)
        {
            var reports = _context.Reports.Where(r => r.Status == reportParams.Status).AsQueryable();
            if (reportParams.Gender == "male" || reportParams.Gender == "female")
            {
                reports = reports.Where(r => _context.Users.Where(u => u.Id == r.UserId).Select(u => u.Gender).First() == reportParams.Gender);
            }
            reports = reports.OrderByDescending(r => r.ReportSent);

            return await PagedList<Report>.CreateAsync(reports, reportParams.PageNumber, reportParams.PageSize);
        }

        // Create report
        public async Task<Report> Create(int userId, NewReportRequest model)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == model.UserId))
            {
                throw new KeyNotFoundException("User not found");
            }
            if (await _context.Reports.AnyAsync(r => r.SenderId == userId && r.UserId == model.UserId))
            {
                return null;
            }

            model.SenderId = userId;
            model.Status = "Pending";
            var report = _mapper.Map<Report>(model);
            _context.Add(report);
            if (await _context.SaveChangesAsync() > 0)
            {
                return report;
            }
            throw new AppException("Failed to send report");
        }

        // Cout by status
        public async Task<int[]> CountByStatus()
        {
            var pending = await _context.Reports.Where(r => r.Status == ReportStatus.Pending).CountAsync();
            var approved = await _context.Reports.Where(r => r.Status == ReportStatus.Approved).CountAsync();
            var disapproved = await _context.Reports.Where(r => r.Status == ReportStatus.Disapproved).CountAsync();

            return new int[] { pending, approved, disapproved };
        }

        // Update status
        public async Task UpdateStatus(int id, UpdateStatusRequest model)
        {
            if (!await _context.Reports.AnyAsync(r => r.Id == id))
            {
                throw new KeyNotFoundException("User not found");
            }

            var reportInDb = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
            if (reportInDb == null)
            {
                throw new KeyNotFoundException("Report not found");
            }
            reportInDb.Status = (ReportStatus)Enum.Parse(typeof(ReportStatus), model.Status);

            _context.Reports.Update(reportInDb);

            if (await _context.SaveChangesAsync() <= 0)
            {
                throw new AppException("Failed to send report");
            }
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
