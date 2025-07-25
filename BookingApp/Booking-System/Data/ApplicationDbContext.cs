using Booking_System.Data.Seed;
using Booking_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Booking_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<UserCoupon> UserCoupons { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventTicketType> EventTicketTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventTicketType>()
                .HasOne(ett => ett.Event)
                .WithMany(e => e.EventTicketTypes)
                .HasForeignKey(ett => ett.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventTicketType>()
                .HasOne(ett => ett.TicketType)
                .WithMany(tt => tt.EventTicketTypes)
                .HasForeignKey(ett => ett.TicketTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventTicketType>()
                .HasIndex(ett => new { ett.EventId, ett.TicketTypeId })
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.EventTicketType)
                .WithMany(ett => ett.Bookings)
                .HasForeignKey(b => b.EventTicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Coupon)
                .WithMany()
                .HasForeignKey(b => b.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.Property(c => c.Code)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(c => c.DiscountPercent)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();
                entity.Property(c => c.MinOrderValue)
                    .HasColumnType("decimal(10,2)");
                entity.Property(c => c.IsActive)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<UserCoupon>(entity =>
            {
                entity.HasKey(uc => uc.Id);
                entity.HasOne(uc => uc.User)
                    .WithMany()
                    .HasForeignKey(uc => uc.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(uc => uc.Coupon)
                    .WithMany(c => c.UserCoupons)
                    .HasForeignKey(uc => uc.CouponId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(uc => new { uc.UserId, uc.CouponId })
                    .IsUnique();
            });
            BookingSystemSeed.SeedModelData(modelBuilder);
        }

    }
}
