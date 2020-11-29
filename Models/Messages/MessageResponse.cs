using System;
namespace DatingApp.API.Models.Messages
{
    public class MessageResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        public string Type { get; set; }
    }
}
