using DatingApp.API.Entities;
using System;
namespace DatingApp.API.Helpers.RequestParams
{
    public class ReportParams : PaginationParams
    {
        public ReportStatus Status { get; set; }
        public ReportParams() : base(10) { }
    }
}
