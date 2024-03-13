using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

    }
}
