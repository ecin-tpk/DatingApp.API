using DatingApp.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Interest> Interests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Like>().HasKey(k => new { k.LikerId, k.LikeeId });
            builder.Entity<Like>().HasOne(u => u.Likee).WithMany(u => u.Likers).HasForeignKey(u => u.LikeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Like>().HasOne(u => u.Liker).WithMany(u => u.Likees).HasForeignKey(u => u.LikerId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>().HasOne(u => u.Sender).WithMany(m => m.MessagesSent).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Message>().HasOne(u => u.Recipient).WithMany(m => m.MessagesReceived).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Interest>().HasKey(k => new { k.UserId, k.ActivityId });
            builder.Entity<Interest>().HasOne(i => i.User).WithMany(u => u.Activities).HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Interest>().HasOne(i => i.Activity).WithMany(a => a.Users).HasForeignKey(a => a.ActivityId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
