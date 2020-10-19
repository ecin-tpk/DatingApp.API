using DatingApp.API.Entities;
using DatingApp.API.Models.Messages;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    public interface IMessagesClient
    {
        Task ReceiveMessage(MessageResponse message);
    }
}
