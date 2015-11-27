using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            #region Adding Entity Framework v7
            services.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<ChinaTowerContext>(x => x.UseSqlite($"Data source={PlatformServices.Default.Application.ApplicationBasePath}/Database/chinatower.db"));
            #endregion

            #region Adding Identity v3
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ChinaTowerContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Adding Others
            services.AddSmartCookies();
            services.AddSmartUser<User, string>();
            services.AddAesCrypto();
            services.AddSmtpEmailSender("smtp.ym.163.com", 25, "中国铁塔", "noreply@vnextcn.org", "noreply@vnextcn.org", "123456");
            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            #region Using middlewares
            app.UseIISPlatformHandler();
            app.UseIdentity();
            app.UseAutoAjax();
            app.UseMvcWithDefaultRoute();
            #endregion
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}