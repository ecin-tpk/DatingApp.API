using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Helpers.RequestParams;
using DatingApp.API.Helpers.Attributes;
using DatingApp.API.Hubs;
using DatingApp.API.Models.Messages;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IMessageService _messageService;

        //private readonly IHubContext<MessagesHub, IMessageClient> _messagesHub;

        private readonly IHubContext<MessagesHub> _messagesHub;

        private readonly IMapper _mapper;

        public MessagesController(IMessageService messageService, IHubContext<MessagesHub> messagesHub, IMapper mapper)
        {
            _messageService = messageService;
            _messagesHub = messagesHub;
            _mapper = mapper;
        }

        // GET: Get message by id
        [HttpGet("{id}", Name = "GetMessageById")]
        public async Task<IActionResult> GetById(int userId, int id)
        {
            // Users can see their own data and admins can see any user's data
            if (userId != User.Id && User.Role != Role.Admin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var message = await _messageService.GetById(id);
            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        // GET: Get messages from matched users (paginated)
        [HttpGet]
        public async Task<IActionResult> GetMessages(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            messageParams.UserId = userId;

            var messages = await _messageService.GetMessages(messageParams);

            Response.AddPagination(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(_mapper.Map<IEnumerable<MessageResponse>>(messages));
        }

        // GET: Get message thread (paginated)
        [HttpGet("thread/{recipientId:int}")]
        public async Task<IActionResult> GetMessageThread(int recipientId, [FromQuery] MessageThreadParams msgThreadParams)
        {
            msgThreadParams.UserId = User.Id;
            msgThreadParams.RecipientId = recipientId;

            var messages = await _messageService.GetMessageThread(msgThreadParams);

            Response.AddPagination(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(_mapper.Map<IEnumerable<NewMessageResponse>>(messages));
        }

        // POST: Send messages
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, NewMessageRequest model)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var message = await _messageService.Create(userId, model);

            await _messagesHub.Clients.User(model.RecipientId.ToString()).SendAsync("receiveMessage", message);

            return CreatedAtRoute("GetMessageById", new { userId, id = message.Id }, message);
        }

        // POST (Remove Message From Sender Or Recipient)
        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _messageService.Delete(id, userId);

            return Ok("Message successfully deleted");
        }

        // POST (Mark Message As Read)
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != User.Id)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await _messageService.MarkAsRead(id, userId);

            return Ok("Message marked as read");
        }
    }
}
