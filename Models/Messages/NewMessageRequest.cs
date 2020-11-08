using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models.Messages
{
    public class NewMessageRequest
    {
        public int SenderId { get; set; }

        public int RecipientId { get; set; }

        public DateTime MessageSent { get; set; }

        public string Content { get; set; }

        public NewMessageRequest()
        {
            MessageSent = DateTime.Now;
        }
    }
}
