using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public int Sender { get; set; }
        public int Receiver { get; set; }
        
        public string Msg {  get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
