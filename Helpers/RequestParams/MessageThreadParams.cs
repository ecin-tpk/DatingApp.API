namespace DatingApp.API.Helpers.RequestParams
{
    public class MessageThreadParams : PaginationParams
    {
        public int UserId { get; set; }
        public int RecipientId { get; set; }

        public MessageThreadParams() : base(20) { }
    }
}
