using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace DatingApp.API.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
        }
    }
}
