using Microsoft.AspNetCore.Identity;
using MyBookShop.Models.Identity;

namespace MyBookShop.Services.SeedData
{
    public static class SeedAdmin
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MyApplicationUser>>();

            string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            string adminUserName = "admin";
            string adminEmail = "admin@MyBookShop.com";
            string adminPassword = "Admin123456**";
            string adminNationalCode = "0000000000";
            string adminFullName = "Application Admin";

            var adminUser = await userManager.FindByNameAsync(adminUserName);
            if (adminUser is null)
            {
                adminUser = new MyApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    NationalCode = adminNationalCode,
                    FullName = adminFullName
                };

                await userManager.CreateAsync(adminUser, adminPassword);
            }

            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}
