using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Example.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Example.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext db;
        public HomeController(ApplicationContext context)
        {
            db = context;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Content(User.Identity.Name);
            }
            return Content("Not autthenticated");
        }

        public async Task<IActionResult> Table()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View(await db.Users.ToListAsync());
            }
            return Content("Not autthenticated");
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]

        // CREATE
        public async Task<IActionResult> Create(User user)
        {
            user.RegistrationDate = DateTime.Now;
            user.LastSeenOnline = DateTime.Now.ToLocalTime();
            user.Status = "Active";
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Table");
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }

        // EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Table");
        }

        // DELETE
        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Table");
                }
            }
            return NotFound();
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
            return RedirectToAction("Table");
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
                    return RedirectPermanent("/Account/Login");
                }
            }
            return RedirectToAction("Table");
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
            return RedirectToAction("Table");
        }

    }
}
