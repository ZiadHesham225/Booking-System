using Booking_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Data.Seed
{
    public static class BookingSystemSeed
    {
        public static void SeedModelData(ModelBuilder builder)
        {
            // Seed TicketTypes
            builder.Entity<TicketType>().HasData(
                new TicketType { TicketTypeId = 1, Name = "Standard", IsActive = true },
                new TicketType { TicketTypeId = 2, Name = "VIP", IsActive = true },
                new TicketType { TicketTypeId = 3, Name = "Student", IsActive = true }
            );

            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Concert" },
                new Category { CategoryId = 2, Name = "Workshop" },
                new Category { CategoryId = 3, Name = "Seminar" },
                new Category { CategoryId = 4, Name = "Tech Talk" }
            );
        }

        public static async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin User
            var adminEmail = "admin@booking.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed Normal User
            var userEmail = "user@booking.com";
            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                var normalUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Doe"
                };

                await userManager.CreateAsync(normalUser, "User123!");
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }
    }
}
