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
        public IActionResult Index()
        {
            return View(UserManager.Users.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = UserManager.Users.Where(x => x.Id == id).Single();
            var cities = (await UserManager.GetClaimsAsync(user))
                .Where(x => x.Type == "有权限访问地市数据")
                .Select(x => x.Value)
                .ToList();
            var citiesstr = "";
            foreach (var x in cities)
                citiesstr += x + " ";
            ViewBag.Cities = citiesstr.Trim();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, User Model, string password, string cities)
        {
            var user = UserManager.Users.Where(x => x.Id == id).Single();
            var claims = await UserManager.GetClaimsAsync(user);
            foreach (var x in claims)
                await UserManager.RemoveClaimAsync(user, x);
            foreach (var x in cities.Trim().Split(' '))
                if (!string.IsNullOrEmpty(x))
                    await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("有权限访问地市数据", x));
            if (!string.IsNullOrEmpty(password))
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                await UserManager.ResetPasswordAsync(user, token, password);
            }
            return Prompt(x => 
            {
                x.Title = "修改成功";
                x.Details = "用户修改成功";
            });
        }

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

        public async Task<IActionResult> Delete(string id)
        {
            if(!User.IsInRole("Root, Master"))
                return Prompt(x =>
                {
                    x.Title = "删除失败";
                    x.Details = "权限不足";
                });
            if (id == User.Current.Id)
                return Prompt(x =>
                {
                    x.Title = "删除失败";
                    x.Details = "你不能将自己的账号删除";
                });
            await UserManager.DeleteAsync(await UserManager.FindByIdAsync(id));
            return Prompt(x => {
                x.Title = "删除成功";
                x.Details = "该用户已被删除";
            });
        }
    }
}
