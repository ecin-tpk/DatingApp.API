using DatingApp.API.Entities;
using DatingApp.API.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DatingApp.API.Hubs
{
    public class MessagesHub : Hub<IMessagesClient>
    {
    }
}
