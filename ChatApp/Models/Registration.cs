using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Registration
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Cpassword { get; set; }
    }
}
