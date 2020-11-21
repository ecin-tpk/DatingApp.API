using DatingApp.API.Models.Messages;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DatingApp.API.Hubs
{
    public interface IMessageClient
    {
        Task ReceiveMessage(NewMessageResponse message);
    }

    public class MessagesHub : Hub<IMessageClient>
    {
    }
}
