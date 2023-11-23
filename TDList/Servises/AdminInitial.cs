using Microsoft.AspNetCore.Identity;
using TDList.Models;

namespace TDList.Services
{
    public class AdminInitial
    {
        public static async Task SeedAdminUser(RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
        {
            try
            {
                string adminEmail = "admin@mail.ru";
                string adminPassword = "123!A123";
                string adminLogin = "AdminaSosok22";
                var roles = new[] { "Admin", "Manager", "Member" };

                foreach (var roleName in roles)
                {
                    if (await roleManager.FindByNameAsync(roleName) == null)
                        await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    User admin = new() { Email = adminEmail, UserName = adminLogin, Login = adminLogin, RoleName = "Admin", IsConfirmed = true };
                    var result = await userManager.CreateAsync(admin, adminPassword);

                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(admin, "MainAdmin");
                    else
                        throw new Exception($"Failed to create admin user. Errors: {string.Join(", ", result.Errors)}");
                }
            }
            catch (Exception ex){Console.Error.WriteLine($"An error occurred during admin user seeding: {ex.Message}");}
        }
    }
}