namespace CinemaBooking.Persistence.Data;

using CinemaBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole("Customer"));

        if (!userManager.Users.Any())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@cinemabooking.com",
                Email = "admin@cinemabooking.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
