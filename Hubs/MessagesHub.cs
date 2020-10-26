using DatingApp.API.Models.Messages;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DatingApp.API.Hubs
{
    public interface IMessageClient
    {
        Task ReceiveMessage(MessageResponse message);
    }

    public class MessagesHub : Hub<IMessageClient>
    {
    }
}
