using System.Collections.Generic;

namespace DatingApp.API.Entities
{
    public class Activity
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public ICollection<Interest> Users { get; set; }
    }
}
