using System;
namespace DatingApp.API.Models.Photos
{
    public class PhotoResponse
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public byte Order { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
    }
}
