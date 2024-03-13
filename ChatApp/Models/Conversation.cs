using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        public int P1Id { get; set; }

        public int P2Id { get; set; }

        public string LastMessage { get; set; } // Last message exchanged in the conversation
        public DateTime LastActivityTimestamp { get; set; } = DateTime.Now;

        // Method to change only lastMsg
        public void ChangeLastMsgProperty(Message newMsg)
        {
            LastMessage = newMsg.Msg;
            LastActivityTimestamp = DateTime.Now;
        }

    }
}
