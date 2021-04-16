using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Example.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Example.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext db;
        public static User ActiveUser = new User();
        public AccountController(ApplicationContext context)
        {
            db = context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    //
                    ActiveUser.Id = user.Id;
                    await Authenticate(model.Username); // authenticate
                    if (user.Status == "Blocked")
                    {
                        ModelState.AddModelError("", "User is blocked");
                        return View(model);
                    }
                    user.LastSeenOnline = DateTime.Now;
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Table", "Home");
                }
                ModelState.AddModelError("", "Wrong username or password or both");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (user == null)
                {
                    // добавляем пользователя в бд
                    db.Users.Add(new User
                    {
                        Username = model.Username,
                        Password = model.Password,
                        Mail = model.Mail,
                        RegistrationDate = DateTime.Now,
                        LastSeenOnline = DateTime.Now,
                        Status = "Active"
                    });
                    await db.SaveChangesAsync();

                    await Authenticate(model.Username); // authenticate

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Wrong username or password or both");
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Account/Login"); 
        }

        // Delete
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] ids)
        {
            foreach (var id in ids)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                }
            }
            for (int i = 0; i < ids.Length; i++)
            {
                if (AccountController.ActiveUser.Id == ids[i])
                {
                    await Logout();
                }
            }
            return RedirectToAction("Table", "Home");
        }

        // Block
        [HttpPost]
        public async Task<IActionResult> BlockSelected(int[] ids)
        {
            foreach (var id in ids)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    user.Status = "Blocked";
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                }
            }
            for (int i = 0; i < ids.Length; i++)
            {
                if (AccountController.ActiveUser.Id == ids[i])
                {
                    await Logout();
                    return RedirectToAction("Login", "Account");
                }
            }
            return RedirectToAction("Table", "Home");
        }
        // Unblock
        [HttpPost]
        public async Task<IActionResult> UnblockSelected(int[] ids)
        {
            foreach (var id in ids)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    user.Status = "Active";
                    db.Users.Update(user);
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("Table", "Home");
        }
    }
}
