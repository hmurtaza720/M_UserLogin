using M_UserLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace M_UserLogin.Controllers
{
    [Authorize(Roles = "Admin")] // ✅ Only admin users can access this controller
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ List all users
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoleMap = new List<(Users User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoleMap.Add((user, roles));
            }

            ViewBag.UserRoles = userRoleMap;
            return View(users);
        }

        // ✅ Assign role to a user
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                TempData["Message"] = "⚠️ Please enter a valid role name.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Message"] = "❌ User not found.";
                return RedirectToAction("Index");
            }

            // Create role if it doesn’t exist
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            // Remove all existing roles so only the new role remains
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Assign the new role
            await _userManager.AddToRoleAsync(user, role);

            TempData["Message"] = $"✅ Role '{role}' assigned to {user.FullName}.";

            // Refresh roles for view immediately
            var users = _userManager.Users.ToList();
            var userRoleMap = new List<(Users User, IList<string> Roles)>();
            foreach (var u in users)
            {
                var rolesList = await _userManager.GetRolesAsync(u);
                userRoleMap.Add((u, rolesList));
            }
            ViewBag.UserRoles = userRoleMap;

            return RedirectToAction("Index");
        }

        // ✅ Delete a user
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Message"] = "❌ User not found.";
                return RedirectToAction("Index");
            }

            // Prevent admin from deleting themselves
            if (user.Email == User.Identity?.Name)
            {
                TempData["Message"] = "⚠️ You cannot delete your own account.";
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(user);

            TempData["Message"] = $"🗑️ User '{user.FullName}' deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
