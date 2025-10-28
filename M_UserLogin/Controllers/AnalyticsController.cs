using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using M_UserLogin.Models;
using System.Linq;
using System.Text.Json;

namespace M_UserLogin.Controllers
{
    [Authorize(Roles = "Admin")]  // 🧩 Only Admins can access this controller
    public class AnalyticsController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AnalyticsController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = _userManager.Users.ToList();
            var totalUsers = users.Count;
            var admins = new List<Users>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    admins.Add(user);
            }

            var roleDistribution = new Dictionary<string, int>();
            foreach (var role in _roleManager.Roles)
            {
                roleDistribution[role.Name] = users.Count(u => _userManager.IsInRoleAsync(u, role.Name).Result);
            }

            var emailDomains = users
                .Select(u => u.Email.Split('@').Last())
                .GroupBy(d => d)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.RoleDistributionJson = JsonSerializer.Serialize(roleDistribution);
            ViewBag.EmailDomainJson = JsonSerializer.Serialize(emailDomains);

            var model = new AnalyticsViewModel
            {
                TotalUsers = totalUsers,
                AdminCount = admins.Count,
                UsersList = users
            };

            return View(model);
        }
    }
}
