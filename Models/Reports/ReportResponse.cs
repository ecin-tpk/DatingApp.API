using System;
namespace DatingApp.API.Models.Reports
{
    public class ReportResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PhotoUrl { get; set; }
        public string Status { get; set; }
        public string ReportedFor { get; set; }
        public DateTime ReportSent { get; set; }
    }
}
