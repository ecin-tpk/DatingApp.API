using System;

namespace DatingApp.API.Entities
{
    public class Like
    {
        public int LikerId { get; set; }
        public int LikeeId { get; set; }
        public User Liker { get; set; }
        public User Likee { get; set; }
        public bool Super { get; set; }
        public bool Unmatched { get; set; }
    }
}
