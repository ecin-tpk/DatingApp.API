using DatingApp.API.Models.Messages;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DatingApp.API.Hubs
{
    public class MessagesHub : Hub
    {
        public async Task SendMessage(MessageResponse message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
