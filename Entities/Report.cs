using System;
namespace DatingApp.API.Entities
{
    public enum ReportStatus
    {
        Pending,
        Approved,
        Disapproved
    }

    public class Report
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int UserId { get; set; }
        public string ReportedFor { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime ReportSent { get; set; }
    }
}
