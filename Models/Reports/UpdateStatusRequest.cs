using DatingApp.API.Entities;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Reports
{
    public class UpdateStatusRequest
    {
        [Required]
        [EnumDataType(typeof(ReportStatus))]
        public string Status { get; set; }
    }
}
