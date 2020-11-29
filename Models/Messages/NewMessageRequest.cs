using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Messages
{
    public class NewMessageRequest
    {
        public int SenderId { get; set; }

        [Required]
        public int RecipientId { get; set; }

        [Required]
        [EnumDataType(typeof(MessageType))]
        public string Type { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime MessageSent { get; set; }

        public NewMessageRequest()
        {
            MessageSent = DateTime.Now;
        }
    }
}
