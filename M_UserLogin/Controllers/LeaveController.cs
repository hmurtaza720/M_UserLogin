using M_UserLogin.Data;
using M_UserLogin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace M_UserLogin.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public LeaveController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ View user’s own leaves
        public async Task<IActionResult> MyLeaves()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var leaves = await _context.LeaveRequests
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            return View(leaves);
        }

        // ✅ Show Apply page
        [HttpGet]
        public IActionResult Apply()
        {
            return View(new LeaveRequest());
        }

        // ✅ Handle Apply form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveRequest model)
        {
            // Forcefully clear model state before validation (to bypass binding issues)
            ModelState.Clear();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "⚠️ User not found. Please log in again.";
                return RedirectToAction("Apply");
            }

            // Reassign core properties manually (so even if form binding fails, we handle it)
            model.UserId = user.Id;
            model.RequestDate = DateTime.Now;
            model.Status = "Pending";

            // Basic manual validation
            if (string.IsNullOrWhiteSpace(model.LeaveType) ||
                model.StartDate == default ||
                model.EndDate == default ||
                string.IsNullOrWhiteSpace(model.Reason))
            {
                TempData["Error"] = "⚠️ Please complete all fields properly.";
                return View(model);
            }

            // Save
            try
            {
                _context.LeaveRequests.Add(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Leave request submitted successfully!";
                return RedirectToAction("MyLeaves");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error while saving leave: {ex.Message}";
                return View(model);
            }
        }

        // ✅ Admin — Manage all leaves
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var leaves = await _context.LeaveRequests
                .Include(l => l.User)
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            return View(leaves);
        }

        // ✅ Approve Leave
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                leave.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Leave for {leave.UserId} approved.";
            }

            return RedirectToAction("Manage");
        }

        // ❌ Reject Leave
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                leave.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["Message"] = $"❌ Leave for {leave.UserId} rejected.";
            }

            return RedirectToAction("Manage");
        }
    }
}
