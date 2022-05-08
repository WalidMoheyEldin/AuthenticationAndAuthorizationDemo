using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthDemo.WebShared.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AuthDemo.Web.ViewModels;

namespace AuthDemo.Web.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public RolesController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        // GET: Roles
        [Authorize(Policy = "Read Roles")]
        public async Task<IActionResult> Index()
        {
            return View(await db.Roles.ToListAsync());
        }

        // GET: Roles/Details/5
        [Authorize(Policy = "Read Roles")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await db.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            var claims = await roleManager.GetClaimsAsync(role);
            ViewData["Policies"] = claims.Select(c => c.Value).ToList();

            ViewData["Users"] = await userManager.GetUsersInRoleAsync(role.Name);

            return View(role);
        }

        // GET: Roles/Create
        [Authorize(Policy = "Create Roles")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Create Roles")]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await roleManager.CreateAsync(new IdentityRole { Name = model.Name });
                if (result.Succeeded)
                {
                    if (model.Policies != null)
                    {
                        var role = await roleManager.FindByNameAsync(model.Name);
                        foreach (string policy in model.Policies)
                        {
                            await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(policy, policy));
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
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Roles/Edit/5
        [Authorize(Policy = "Update Roles")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var claims = await roleManager.GetClaimsAsync(role);

            EditRoleViewModel model = new EditRoleViewModel
            {
                Id = id,
                Name = role.Name,
                Policies = claims.Select(c => c.Value).ToList()
            };

            return View(model);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Update Roles")]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await db.Roles.FindAsync(model.Id);
                role.Name = model.Name;
                await db.SaveChangesAsync();

                var claims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    await roleManager.RemoveClaimAsync(role, claim);
                }

                if (model.Policies != null)
                {
                    foreach (string policy in model.Policies)
                    {
                        await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(policy, policy));
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Roles/Delete/5
        [Authorize(Policy = "Delete Roles")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await db.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            var claims = await roleManager.GetClaimsAsync(role);
            ViewData["Policies"] = claims.Select(c => c.Value).ToList();

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Delete Roles")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await db.Roles.FindAsync(id);
            db.Roles.Remove(role);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
