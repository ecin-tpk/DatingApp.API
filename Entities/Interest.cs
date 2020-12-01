namespace DatingApp.API.Entities
{
    public class Interest
    {
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public User User { get; set; }
        public Activity Activity { get; set; }
    }
}
