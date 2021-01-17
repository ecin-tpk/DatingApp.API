using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IChartService
    {
        Task<TimeValueChartData> GetNewUsersPerMonth(double milliseconds);
        Task<TimeValueChartData> GetTotalUsers(double milliseconds, string period);
        Task<TimeValueChartDataTest> GetActivePercentage(double milliseconds);
    }
    #endregion

    public class ChartService : IChartService
    {
        private readonly DataContext _context;

        public ChartService(DataContext context)
        {
            _context = context;
        }

        // Get new users per month (12)
        public async Task<TimeValueChartData> GetNewUsersPerMonth(double milliseconds)
        {
            TimeSpan dateTimeSpan = TimeSpan.FromMilliseconds(milliseconds);
            DateTime dateAfterEpoch = new DateTime(1970, 1, 1) + dateTimeSpan;
            var data = new TimeValueChartData();
            var dates = new string[12];
            var values = new int[12];
            for (int i = 0; i < 12; i++)
            {
                var month = dateAfterEpoch.ToLocalTime().AddMonths(-i);
                var total = await _context.Users
                    .Where(u => u.Created.Month == month.Month && u.Created.Year == month.Year).CountAsync();
                dates[11 - i] = month.ToString("MMM yy");
                values[11 - i] = total;
            }
            data.Dates = dates;
            data.Values = values;
            return data;
        }

        // Get total users (12)
        public async Task<TimeValueChartData> GetTotalUsers(double milliseconds, string period)
        {
            TimeSpan dateTimeSpan = TimeSpan.FromMilliseconds(milliseconds);
            DateTime dateAfterEpoch = new DateTime(1970, 1, 1) + dateTimeSpan;
            var data = new TimeValueChartData();
            string[] dates;
            int[] values;
            switch (period)
            {
                case "quarterly":
                    dates = new string[4];
                    values = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        var date = dateAfterEpoch.ToLocalTime().AddMonths(-3 * i);
                        var total = await GetQuarterlyTotalUsers(date);
                        dates[3 - i] = $@"Q{date.GetQuarter()} {date:yy}";
                        values[3 - i] = total;
                    }
                    data.Dates = dates;
                    data.Values = values;
                    break;
                case "daily":
                    dates = new string[7];
                    values = new int[7];
                    for (int i = 0; i < 7; i++)
                    {
                        var date = dateAfterEpoch.ToLocalTime().AddDays(-i);
                        var total = await _context.Users
                            .Where(u => u.Created <= date).CountAsync();
                        dates[6 - i] = date.DayOfWeek.ToString();
                        values[6 - i] = total;
                    }
                    data.Dates = dates;
                    data.Values = values;
                    break;
                default:
                    dates = new string[12];
                    values = new int[12];
                    for (int i = 0; i < 12; i++)
                    {
                        var date = dateAfterEpoch.ToLocalTime().AddMonths(-i);
                        var total = await _context.Users
                            .Where(u => u.Created <= date).CountAsync();
                        dates[11 - i] = date.ToString("MMM yy");
                        values[11 - i] = total;
                    }
                    data.Dates = dates;
                    data.Values = values;
                    break;
            }
            return data;
        }

        // Get active percentage
        public async Task<TimeValueChartDataTest> GetActivePercentage(double milliseconds)
        {
            var date = milliseconds.GetLocalTime();
            var data = new TimeValueChartDataTest
            {
                Dates = new List<string>(),
                Values = new List<double>()
            };
            int i = 0;
            while (date.AddHours(-i).Day == date.Day)
            {
                ((List<string>)data.Dates).Add(date.AddHours(-i).ToString());
                ((List<double>)data.Values).Add(await GetActivePercentage(date.AddHours(-i)));
                i++;
                if (date.AddHours(-i).Day < date.Day && date.Minute > 0)
                {
                    var start = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
                    ((List<string>)data.Dates).Add(start.ToString());
                    ((List<double>)data.Values).Add(await GetActivePercentage(start));
                }
            }
            ((List<string>)data.Dates).Reverse();
            ((List<double>)data.Values).Reverse();
            return data;
        }

        // Get quarterly total users
        private async Task<int> GetQuarterlyTotalUsers(DateTime date)
        {
            DateTime firstDay;
            switch (date.GetQuarter())
            {
                case 1:
                    firstDay = new DateTime(date.Year, 4, 1, 0, 0, 0, 0);
                    return await _context.Users.Where(u => u.Created < firstDay).CountAsync();
                case 2:
                    firstDay = new DateTime(date.Year, 7, 1, 0, 0, 0, 0);
                    return await _context.Users.Where(u => u.Created < firstDay).CountAsync();
                case 3:
                    firstDay = new DateTime(date.Year, 10, 1, 0, 0, 0, 0);
                    return await _context.Users.Where(u => u.Created < firstDay).CountAsync();
                default:
                    firstDay = new DateTime(date.Year + 1, 1, 1, 0, 0, 0, 0);
                    return await _context.Users.Where(u => u.Created < firstDay).CountAsync();
            }
        }

        private async Task<double> GetActivePercentage(DateTime date)
        {
            var active = await _context.Users.Where
            (
                u =>
                u.LastActive.Date == date.Date &&
                u.LastActive.Hour == date.Hour &&
                u.Role != Role.Admin
            ).CountAsync();
            var total = await _context.Users.Where(u => u.Role != Role.Admin).CountAsync();
            return (double)active * 100 / total;
        }
    }

    public class TimeValueChartData
    {
        public string[] Dates { get; set; }
        public int[] Values { get; set; }
    }

    public class TimeValueChartDataTest
    {
        public IEnumerable Dates { get; set; }
        public IEnumerable Values { get; set; }
    }
}
