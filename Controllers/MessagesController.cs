using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
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

        //[HttpGet]
        //public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        //{
        //    if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //    {
        //        return Unauthorized();
        //    }

        //    messageParams.UserId = userId;

        //    var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

        //    var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

        //    Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

        //    return Ok(messages);
        //}

        // GET: Get message thread (paginated)
        [HttpGet("thread/{recipientId:int}")]
        public async Task<IActionResult> GetPagination(int recipientId, [FromQuery] MessageThreadParams msgThreadParams)
        {
            msgThreadParams.UserId = User.Id;
            msgThreadParams.RecipientId = recipientId;

            var messages = await _messageService.GetPagination(msgThreadParams);

            Response.AddPagination(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(_mapper.Map<IEnumerable<MessageResponse>>(messages));
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

            //await _messagesHub.Clients.All.ReceiveMessage(message);

            await _messagesHub.Clients.All.SendAsync("ReceiveMessage", message);

            return CreatedAtRoute("GetMessageById", new { userId, id = message.Id }, message);
        }

        //// POST (Remove Message From Sender Or Recipient)
        //[HttpPost("{id}")]
        //public async Task<IActionResult> DeleteMessage(int id, int userId)
        //{
        //    if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //    {
        //        return Unauthorized();
        //    }

        //    var messageFromRepo = await _repo.GetMessage(id);

        //    if(messageFromRepo.SenderId == userId)
        //    {
        //        messageFromRepo.SenderDeleted = true;
        //    }

        //    if (messageFromRepo.RecipientId == userId)
        //    {
        //        messageFromRepo.RecipientDeleted = true;
        //    }

        //    // If both sender and recipient deleted the message, then go and delete the message in database as well
        //    if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
        //    {
        //        _repo.Delete(messageFromRepo);
        //    }

        //    if(await _repo.SaveAll())
        //    {
        //        return NoContent();
        //    }

        //    throw new Exception("Error deleting the message");
        //}

        //// POST (Mark Message As Read)
        //[HttpPost("{id}/read")]
        //public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        //{
        //    if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //    {
        //        return Unauthorized();
        //    }

        //    var message = await _repo.GetMessage(id);

        //    if(message.RecipientId != userId)
        //    {
        //        return Unauthorized();
        //    }

        //    message.IsRead = true;
        //    message.DateRead = DateTime.Now;

        //    await _repo.SaveAll();

        //    return NoContent();
        //}
    }
}
