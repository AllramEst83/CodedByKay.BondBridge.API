using CodedByKay.BondBridge.API.Models.DBModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodedByKay.BondBridge.API.DBContext
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Log> Logs { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<ConversationUser> ConversationUsers { get; set; }
        public DbSet<Message> Messages { get; set; }


        //Join tables
        public DbSet<UserGroup> UserGroup { get; set; }


    }
}
