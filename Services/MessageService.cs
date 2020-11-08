using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Hubs;
using DatingApp.API.Models.Messages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IMessageService
    {
        Task<Message> GetById(int id);
        Task<MessageResponse> Create(int userId, NewMessageRequest model);
        Task Delete(int id, int userId);
        Task MarkAsRead(int id, int userId);
        Task<PagedList<Message>> GetMessageForUser();
        Task<PagedList<Message>> GetPagination(MessageThreadParams msgThreadparams);
    }
    #endregion

    public class MessageService : IMessageService
    {
        private readonly IUserService _userService;
        private readonly ILikeService _likeService;
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public MessageService(IUserService userService, ILikeService likeService, IMapper mapper, DataContext context)
        {
            _userService = userService;
            _likeService = likeService;
            _mapper = mapper;
            _context = context;
        }

        // Get message by id
        public async Task<Message> GetById(int id)
        {
            return await _context.Messages.SingleOrDefaultAsync(m => m.Id == id);
        }

        // Create new message
        public async Task<MessageResponse> Create(int userId, NewMessageRequest model)
        {
            // Must find sender for automapping
            var user = await _userService.GetUser(userId);

            model.SenderId = user.Id;

            // Check if they are matched or not
            if (await _likeService.AreMatched(model.SenderId, model.RecipientId) == false)
            {
                throw new AppException("Can not send message to an unmatch user");
            }

            if (await _userService.GetUser(model.RecipientId) == null)
            {
                throw new AppException("User not found");
            }

            var message = _mapper.Map<Message>(model);

            _context.Add(message);

            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<MessageResponse>(message);
            }

            throw new AppException("Send messaged failed");
        }

        // Delete message
        public async Task Delete(int id, int userId)
        {
            var messageInDb = await _context.Messages.SingleOrDefaultAsync(m => m.Id == id);
            if (messageInDb.SenderId == userId)
            {
                messageInDb.SenderDeleted = true;
            }
            if (messageInDb.RecipientId == userId)
            {
                messageInDb.RecipientDeleted = true;
            }

            // If both sender and recipient deleted the message, then go and delete the message in database as well
            if (messageInDb.SenderDeleted && messageInDb.RecipientDeleted)
            {
                _context.Remove(messageInDb);
            }

            await _context.SaveChangesAsync();
        }

        // Mark message as read
        public async Task MarkAsRead(int id, int userId)
        {
            var message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == id);
            if (message.RecipientId != userId)
            {
                throw new AppException("Unauthorized");
            }
            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public Task<PagedList<Message>> GetMessageForUser()
        {
            throw new NotImplementedException();
        }

        // Get messages of a thread (paginated)
        public async Task<PagedList<Message>> GetPagination(MessageThreadParams msgThreadparams)
        {
            var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m =>
                    (
                        m.RecipientId == msgThreadparams.UserId &&
                        m.RecipientDeleted == false &&
                        m.SenderId == msgThreadparams.RecipientId
                    ) ||
                    (
                        m.RecipientId == msgThreadparams.RecipientId &&
                        m.SenderDeleted == false &&
                        m.SenderId == msgThreadparams.UserId
                    )
                ).AsQueryable().OrderBy(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, msgThreadparams.PageNumber, msgThreadparams.PageSize);
        }
    }
}
