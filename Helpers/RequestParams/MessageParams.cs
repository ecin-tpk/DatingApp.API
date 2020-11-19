using System;
namespace DatingApp.API.Helpers.RequestParams
{
    public class MessageParams : PaginationParams
    {
        public MessageParams() : base(10) { }
        public int UserId { get; set; }
        public string MessageContainer { get; set; } = "any";
    }
}
