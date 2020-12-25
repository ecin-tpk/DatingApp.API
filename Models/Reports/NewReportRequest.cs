using DatingApp.API.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Reports
{
    public class NewReportRequest
    {
        public int SenderId { get; set; }
        public int UserId { get; set; }

        [EnumDataType(typeof(ReportStatus))]
        public string Status { get; set; }

        [Required]
        public string ReportedFor { get; set; }

        public DateTime ReportSent { get; set; }

        public NewReportRequest()
        {
            ReportSent = DateTime.Now;
        }
    }
}
