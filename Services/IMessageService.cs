using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Models.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    public interface IMessageService
    {
        Task<Message> GetById(int id);

        Task<MessageResponse> Create(int userId, NewMessageRequest model);

        Task<PagedList<Message>> GetMessageForUser();

        Task<IEnumerable<MessageResponse>> GetMessageThread(int userId, int recipientId);

        Task<PagedList<Message>> GetPagination(MessageThreadParams msgThreadparams);
    }
}
