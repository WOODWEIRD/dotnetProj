﻿using System.Text.Json;
using API.Entites;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        _ = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var Users = JsonSerializer.Deserialize<List<AppUser>>(userData);

        var roles = new List<AppRole>{
            new() {Name = "Member"},
            new() {Name = "Admin"},
            new() {Name = "Mod"}
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        foreach (var user in Users)
        {
            user.UserName = user.UserName.ToLower();
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser
        {
            UserName = "admin",
        };
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "Member", "Mod" });


    }
}
