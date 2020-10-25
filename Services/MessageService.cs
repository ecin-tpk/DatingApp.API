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
    public class MessageService : IMessageService
    {
        private readonly IUserService _userService;

        private readonly IMapper _mapper;

        private readonly DataContext _context;

        public MessageService(IUserService userService, IMapper mapper, DataContext context)
        {
            _userService = userService;
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
            model.SenderId = userId;

            var recipient = await _userService.GetUser(model.RecipientId);
            if (recipient == null)
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

        public Task<PagedList<Message>> GetMessageForUser()
        {
            throw new NotImplementedException();
        }

        // Get message thread
        public async Task<IEnumerable<MessageResponse>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId
                || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
                .OrderByDescending(m => m.MessageSent).ToListAsync();

            return _mapper.Map<IEnumerable<MessageResponse>>(messages);
        }

        public async Task<PagedList<Message>> GetPagination(MessageThreadParams msgThreadparams)
        {
            var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.RecipientId == msgThreadparams.UserId && m.RecipientDeleted == false && m.SenderId == msgThreadparams.RecipientId
                || m.RecipientId == msgThreadparams.RecipientId && m.SenderDeleted == false && m.SenderId == msgThreadparams.UserId).AsQueryable()
                .OrderBy(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, msgThreadparams.PageNumber, msgThreadparams.PageSize);
        }
    }
}
