using System;
using System.Threading.Tasks;
using DatingApp.API.Models.Reports;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.Hubs
{
    public interface IReportClient
    {
        Task ReceiveReport(NewReportRequest report);
    }

    public class ReportsHub : Hub<IReportClient>
    {
    }
}
