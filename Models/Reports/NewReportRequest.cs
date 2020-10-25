using System;

namespace DatingApp.API.Models.Reports
{
    public class NewReportRequest
    {
        public int SenderId { get; set; }

        public int UserId { get; set; }

        public string ReportedFor { get; set; }

        public DateTime ReportSent { get; set; }

        public NewReportRequest()
        {
            ReportSent = DateTime.Now;
        }
    }
}
