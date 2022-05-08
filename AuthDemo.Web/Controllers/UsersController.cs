using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthDemo.WebShared.Data;
using AuthDemo.Models;
using AuthDemo.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AuthDemo.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // GET: Users
        [Authorize(Policy = "Read Users")]
        public async Task<IActionResult> Index()
        {
            return View(await db.Users.ToListAsync());
        }

        // GET: Users/Details/5
        [Authorize(Policy = "Read Users")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await db.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["Roles"] = await userManager.GetRolesAsync(user);

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Policy = "Create Users")]
        public async Task<IActionResult> Create()
        {
            ViewData["Roles"] = await db.Roles.ToListAsync();
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Create Users")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true, PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true };
                IdentityResult result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (model.Roles != null)
                    {
                        user = await userManager.FindByNameAsync(user.UserName);
                        foreach (var role in model.Roles)
                        {
                            await userManager.AddToRoleAsync(user, role);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                    ViewData["Roles"] = await db.Roles.ToListAsync();
                    return View(model);
                }
            }

            ViewData["Roles"] = await db.Roles.ToListAsync();
            return View(model);
        }

        // GET: Users/Edit/5
        [Authorize(Policy = "Update Users")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            EditUserViewModel model = new EditUserViewModel { 
                Id = id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            ViewData["Roles"] = await db.Roles.ToListAsync();
            ViewData["UserRoles"] = await userManager.GetRolesAsync(user);
            return View(model);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Update Users")]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users.FindAsync(model.Id);
                user.PhoneNumber = model.PhoneNumber;
                await db.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(model.Password) && !string.IsNullOrWhiteSpace(model.PasswordConfirmation))
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, model.PasswordConfirmation);
                }

                var userRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, userRoles);

                if (model.Roles != null)
                {
                    user = await userManager.FindByNameAsync(user.UserName);
                    foreach (var role in model.Roles)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Users/Delete/5
        [Authorize(Policy = "Delete Users")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await db.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Delete Users")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await db.Users.FindAsync(id);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
