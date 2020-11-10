using System;
namespace DatingApp.API.Models.Reports
{
    public class ReportResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int UserId { get; set; }
        public string ReportedFor { get; set; }
        public DateTime ReportSent { get; set; }
    }
}
