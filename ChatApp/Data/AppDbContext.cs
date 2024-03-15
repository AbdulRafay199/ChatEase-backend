using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<P1LastMessage> P1LastMessages { get; set; }
        public DbSet<P2LastMessage> P2LastMessages { get; set; }
        public DbSet<Message> Messages { get; set; }




    }
}
