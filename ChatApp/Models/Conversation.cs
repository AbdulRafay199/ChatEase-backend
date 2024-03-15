using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatApp.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        public int P1Id { get; set; }

        public int P2Id { get; set; }

        public List<P1LastMessage> P1LastMessages { get; set; } // Last message exchanged in the conversation
        public List<P2LastMessage> P2LastMessages { get; set; } // Last message exchanged in the conversation
        public string LastMessage {  get; set; }
        public DateTime LastActivityTimestamp { get; set; } = DateTime.Now;

        // Method to change only lastMsg
        public void updateLastMessage(Message newMsg)
        {
            LastMessage = newMsg.Msg;
            LastActivityTimestamp = DateTime.Now;
        }

    }

    public class P1LastMessage {
        [Key]
        public int MsgId { get; set; }
        public string Msg { get; set; }

        [ForeignKey("ConversationId")]
        [JsonIgnore]
        public Conversation Conversation { get; set; }
        public int ConversationId { get; set; }
    }

    public class P2LastMessage
    {
        [Key]
        public int MsgId { get; set; }
        public string Msg { get; set; }

        [ForeignKey("ConversationId")]
        [JsonIgnore]
        public Conversation Conversation { get; set; }
        public int ConversationId { get; set; }
    }



}
