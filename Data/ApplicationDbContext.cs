using Auction_web.Models;
using Microsoft.EntityFrameworkCore;

namespace Auction_web.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(180);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UNIQUE CONSTRAINTS
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // RELATIONSHIPS
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Seller)
                .WithMany(u => u.ItemsForSale)
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Item)
                .WithMany(i => i.Bids)
                .HasForeignKey(b => b.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Buyer)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Item)
                .WithMany()
                .HasForeignKey(r => r.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.RatedUser)
                .WithMany(u => u.RatingsReceived)
                .HasForeignKey(r => r.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Item)
                .WithMany(i => i.Notifications)
                .HasForeignKey(n => n.ItemId)
                .OnDelete(DeleteBehavior.SetNull);

            // INDEXES
            modelBuilder.Entity<Item>().HasIndex(i => i.SellerId);
            modelBuilder.Entity<Item>().HasIndex(i => i.CategoryId);
            modelBuilder.Entity<Bid>().HasIndex(b => b.ItemId);
            modelBuilder.Entity<Bid>().HasIndex(b => b.BuyerId);
            modelBuilder.Entity<Notification>().HasIndex(n => n.UserId);
        }
    }
}
