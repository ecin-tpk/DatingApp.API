using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Models.Photos
{
    public class UploadRequest
    {
        public string Url { get; set; }

        [Required]
        public IFormFile File { get; set; }

        public DateTime DateAdded { get; set; }

        public string PublicId { get; set; }

        public UploadRequest()
        {
            DateAdded = DateTime.Now;
        }
    }
}
