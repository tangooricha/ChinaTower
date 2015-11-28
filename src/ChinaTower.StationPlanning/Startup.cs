using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services.AddIdentity<User, IdentityRole>(x => 
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonLetterOrDigit = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddEntityFrameworkStores<ChinaTowerContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Adding Others
            services.AddMvc();
            services.AddSmartCookies();
            services.AddSmartUser<User, string>();
            services.AddAesCrypto();
            services.AddSmtpEmailSender("smtp.ym.163.com", 25, "中国铁塔", "noreply@vnextcn.org", "noreply@vnextcn.org", "123456");
            #endregion
        }

        public async void Configure(IApplicationBuilder app, ILoggerFactory logger)
        {
            #region Setting loggers
            logger.MinimumLevel = LogLevel.Warning;
            logger.AddConsole();
            logger.AddDebug();
            #endregion

            #region Using middlewares
            app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseAutoAjax();
            app.UseMvcWithDefaultRoute();
            #endregion

            #region Init database
            await SampleData.InitDB(app.ApplicationServices);
            #endregion
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}