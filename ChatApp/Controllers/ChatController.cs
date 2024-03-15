using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;

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
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            var conversations = _context.Conversations
                            .Select(c => new
                            {
                                c.Id,
                                c.P1Id,
                                c.P2Id,
                                P1LastMessages = c.P1LastMessages.Select(lm => lm.Msg).ToList(), // Project only Msg strings
                                P2LastMessages = c.P2LastMessages.Select(lm => lm.Msg).ToList(), // Project only Msg strings
                                c.LastMessage,
                                c.LastActivityTimestamp
                            })
                            .ToList();

            var json = JsonSerializer.Serialize(new { Conversations = conversations }, options);
            return Ok(json);
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
                e.Id,
                e.P1Id,
                e.P2Id,
                P1LastMessages = e.P1LastMessages.Select(lm => lm.Msg).ToList(), // Project only Msg strings
                P2LastMessages = e.P2LastMessages.Select(lm => lm.Msg).ToList(), // Project only Msg strings
                e.LastMessage,
                e.LastActivityTimestamp,
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
                if(msg.Sender == convoExist.P1Id)
                {
                    P2LastMessage p = new P2LastMessage() { Msg = msg.Msg, ConversationId = convoExist.Id };
                    _context.P2LastMessages.Add(p);
                    _context.SaveChanges();
                    convoExist.updateLastMessage(msg);
                    _context.Conversations.Update(convoExist);

                    await _context.Messages.AddAsync(msg);
                    await _context.SaveChangesAsync();

                    return Ok(new { msg });
                }
                else
                {
                    P1LastMessage p = new P1LastMessage() { Msg = msg.Msg, ConversationId = convoExist.Id };
                    _context.P1LastMessages.Add(p);
                    _context.SaveChanges();
                    convoExist.updateLastMessage(msg);
                    _context.Conversations.Update(convoExist);

                    await _context.Messages.AddAsync(msg);
                    await _context.SaveChangesAsync();

                    return Ok(new { msg });
                }
                
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

                P2LastMessage p = new P2LastMessage() { Msg = msg.Msg, ConversationId=newConvo.Id };
                _context.P2LastMessages.Add(p);
                _context.SaveChanges();

                return Ok(new { msg });
            }
        }

        [Authorize]
        [HttpDelete("deleteallbutlast/{conversationId}")]
        public async Task<IActionResult> DeleteAllButLastMessages(int conversationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            int.TryParse(userIdClaim, out int userId);
            var conversation = _context.Conversations.FirstOrDefault(e => e.Id == conversationId);

            if (userId == conversation.P1Id)
            {
                // Retrieve the last message for the conversation
/*                var lastMessage = await _context.P1LastMessages
                    .OrderByDescending(m => m.MsgId)
                    .FirstOrDefaultAsync(m => m.ConversationId == conversationId);

                if (lastMessage == null)
                {
                    return NotFound(new { msg = "Not Found" });
                }*/

                // Retrieve all messages for the conversation except the last one
                var messagesToDelete = await _context.P1LastMessages
                    .Where(m => m.ConversationId == conversationId)
                    .ToListAsync();

                // Remove all messages except the last one
                _context.P1LastMessages.RemoveRange(messagesToDelete);
            }
            else
            {
                // Retrieve the last message for the conversation
/*                var lastMessage = await _context.P2LastMessages
                    .OrderByDescending(m => m.MsgId)
                    .FirstOrDefaultAsync(m => m.ConversationId == conversationId);

                if (lastMessage == null)
                {
                    return NotFound(new { msg = "Not Found" });
                }
*/
                // Retrieve all messages for the conversation except the last one
                var messagesToDelete = await _context.P2LastMessages
                    .Where(m => m.ConversationId == conversationId)
                    .ToListAsync();

                // Remove all messages except the last one
                _context.P2LastMessages.RemoveRange(messagesToDelete);
            }

            


            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new {msg = "Messages read"});
        }

        [HttpDelete("deleteall/{conversationId}/{userId}")]
        public async Task<IActionResult> DeleteAll(int conversationId,int userId)
        {
            var conversation = _context.Conversations.FirstOrDefault(e => e.Id == conversationId);

            if (userId == conversation.P1Id)
            {
                
                var messagesToDelete = await _context.P1LastMessages
                    .Where(m => m.ConversationId == conversationId)
                    .ToListAsync();

                _context.P1LastMessages.RemoveRange(messagesToDelete);
            }
            else
            {
                
                var messagesToDelete = await _context.P2LastMessages
                    .Where(m => m.ConversationId == conversationId)
                    .ToListAsync();

                _context.P2LastMessages.RemoveRange(messagesToDelete);
            }




            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { msg = "Messages read" });
        }

    }
}
