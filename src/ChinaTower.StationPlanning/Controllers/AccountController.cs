using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool remember, [FromHeader] string Referer)
        {
            var result = await SignInManager.PasswordSignInAsync(username, password, remember, false);
            if (result.Succeeded)
                return Redirect(Referer == null || Referer.IndexOf("Account") > 0 ? Url.Action("Index", "Home") : Referer);
            else
                return Prompt(x =>
                {
                    x.Title = "登录失败";
                    x.Details = "请检查用户名密码是否正确后返回上一页重试！";
                    x.RedirectText = "忘记密码";
                    x.RedirectUrl = Url.Action("Index", "Home");
                    x.StatusCode = 403;
                });
        }

        [HttpGet]
        public IActionResult Avatar(string id, [FromServices]IHostingEnvironment env)
        {
            var avatar = DB.Users
                .Include(x => x.Avatar)
                .Where(x => x.Id == id)
                .Select(x => x.Avatar)
                .SingleOrDefault();
            if (avatar == null)
                return File(System.IO.File.ReadAllBytes(Path.Combine(env.WebRootPath, "images/non-avatar.png")), "image/x-png", "avatar.png");
            else
                return File(avatar.File, avatar.ContentType, avatar.FileName);
        }
    }
}
