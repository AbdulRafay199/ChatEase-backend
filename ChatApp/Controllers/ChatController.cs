using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChatController(AppDbContext context) 
        {
            _context = context;
        }

        [HttpGet("getallmsgs")]
        public IActionResult GetAllMessages()
        {
            return Ok(new { Messages = _context.Messages.ToList() });
        }

        [HttpGet("getspecificmsgs/{user1}/{user2}")]
        public IActionResult GetSpecificMessages( int user1, int user2)
        {
            var allMsgs = _context.Messages.Where(e => (e.Sender == user1 || e.Sender == user2) && (e.Receiver == user1 || e.Receiver == user2)).ToList();
            return Ok(new { Messages = allMsgs });
        }

        [HttpGet("getallconvo")]
        public IActionResult GetAllConversations()
        {
            return Ok(new { Conversations = _context.Conversations.ToList() });
        }

        [Authorize]
        [HttpGet("getconvobytoken")]
        public IActionResult GetConversationsByToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            int.TryParse(userIdClaim, out int userId);
            var Conversations = _context.Conversations
            .Where(e => e.P1Id == userId || e.P2Id == userId)
            .OrderByDescending(e => e.LastActivityTimestamp)
            .Select(e => new
            {
                Conversation = e,
                OtherUserName = e.P1Id == userId ? _context.Users.FirstOrDefault(u => u.Id == e.P2Id).Name : _context.Users.FirstOrDefault(u => u.Id == e.P1Id).Name
            })
            .ToList();
            return Ok(new { conversations= Conversations });
        }

        [HttpPost]
        public async Task<object> Addmsg(Message msg)
        {
            var convoExist = await _context.Conversations.FirstOrDefaultAsync(e => (e.P1Id == msg.Sender || e.P1Id == msg.Receiver) && (e.P2Id == msg.Sender || e.P2Id == msg.Receiver));
            if (convoExist != null)
            {
                convoExist.ChangeLastMsgProperty(msg);
                _context.Conversations.Update(convoExist);

                await _context.Messages.AddAsync(msg);
                await _context.SaveChangesAsync();

                return Ok(new { msg });
            }
            else
            {
                var newConvo = new Conversation
                {
                    Id = 0,
                    P1Id = msg.Sender,
                    P2Id = msg.Receiver,  
                    LastMessage = msg.Msg
                };
                await _context.Conversations.AddAsync(newConvo);

                await _context.Messages.AddAsync(msg);
                await _context.SaveChangesAsync();

                return Ok(new { msg });
            }
        }
    }
}
