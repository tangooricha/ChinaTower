using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace ChinaTower.StationPlanning.Models
{
    public static class SampleData
    {
        public static async Task InitDB(this IServiceProvider self)
        {
            var DB = self.GetRequiredService<ChinaTowerContext>();
            var UserManager = self.GetRequiredService<UserManager<User>>();
            var RoleManager = self.GetRequiredService<RoleManager<IdentityRole>>();
            if (DB.Database.EnsureCreated())
            {
                await RoleManager.CreateAsync(new IdentityRole("Root"));
                await RoleManager.CreateAsync(new IdentityRole("Master"));
                await RoleManager.CreateAsync(new IdentityRole("Member"));

                var user = new User
                {
                    UserName = "admin"
                };
                await UserManager.CreateAsync(user, "123456");
                await UserManager.AddToRoleAsync(user, "Root");
            }
        }
    }
}
