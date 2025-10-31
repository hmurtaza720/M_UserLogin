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

        private const int DefaultCasual = 12;
        private const int DefaultSick = 6;
        private const int DefaultAnnual = 14;

        public LeaveController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ USER: Apply GET
        [HttpGet]
        public IActionResult Apply()
        {
            return View(new LeaveRequest());
        }

        // ✅ USER: Apply POST with leave balance check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int days = (int)(model.EndDate - model.StartDate).TotalDays + 1;

            // Initialize leave balances if 0
            if (user.CasualLeaveBalance <= 0) user.CasualLeaveBalance = DefaultCasual;
            if (user.SickLeaveBalance <= 0) user.SickLeaveBalance = DefaultSick;
            if (user.AnnualLeaveBalance <= 0) user.AnnualLeaveBalance = DefaultAnnual;

            // Check leave balance
            switch (model.LeaveType.ToLower())
            {
                case "casual leave":
                    if (user.CasualLeaveBalance < days)
                    {
                        TempData["Error"] = $"⚠️ You only have {user.CasualLeaveBalance} Casual Leave(s) remaining.";
                        return RedirectToAction("Apply");
                    }
                    break;
                case "sick leave":
                    if (user.SickLeaveBalance < days)
                    {
                        TempData["Error"] = $"⚠️ You only have {user.SickLeaveBalance} Sick Leave(s) remaining.";
                        return RedirectToAction("Apply");
                    }
                    break;
                case "annual leave":
                    if (user.AnnualLeaveBalance < days)
                    {
                        TempData["Error"] = $"⚠️ You only have {user.AnnualLeaveBalance} Annual Leave(s) remaining.";
                        return RedirectToAction("Apply");
                    }
                    break;
                default:
                    TempData["Error"] = "⚠️ Unknown leave type.";
                    return RedirectToAction("Apply");
            }

            // Save leave request
            model.UserId = user.Id;
            model.Status = "Pending";
            model.RequestDate = DateTime.Now;

            _context.LeaveRequests.Add(model);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Leave request submitted successfully!";
            return RedirectToAction("MyLeaves");
        }

        // ✅ USER: View my leaves
        [HttpGet]
        public async Task<IActionResult> MyLeaves()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var leaves = await _context.LeaveRequests
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            ViewBag.CasualLeave = user.CasualLeaveBalance <= 0 ? DefaultCasual : user.CasualLeaveBalance;
            ViewBag.SickLeave = user.SickLeaveBalance <= 0 ? DefaultSick : user.SickLeaveBalance;
            ViewBag.AnnualLeave = user.AnnualLeaveBalance <= 0 ? DefaultAnnual : user.AnnualLeaveBalance;

            return View(leaves);
        }

        // ✅ ADMIN: Manage all leaves
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var leaves = await _context.LeaveRequests
                .Include(l => l.User)
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            return View(leaves);
        }

        // ✅ ADMIN: Approve leave
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve([FromForm] int id)
        {
            var leave = await _context.LeaveRequests
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
            {
                TempData["Error"] = "❌ Leave request not found.";
                return RedirectToAction("Manage");
            }

            if (leave.Status == "Approved")
            {
                TempData["Error"] = "⚠️ Leave already approved.";
                return RedirectToAction("Manage");
            }

            var user = leave.User;

            // Initialize leave balances if 0
            if (user.CasualLeaveBalance <= 0) user.CasualLeaveBalance = DefaultCasual;
            if (user.SickLeaveBalance <= 0) user.SickLeaveBalance = DefaultSick;
            if (user.AnnualLeaveBalance <= 0) user.AnnualLeaveBalance = DefaultAnnual;

            int days = (int)(leave.EndDate - leave.StartDate).TotalDays + 1;

            switch (leave.LeaveType.ToLower())
            {
                case "casual leave":
                    if (user.CasualLeaveBalance >= days)
                        user.CasualLeaveBalance -= days;
                    else
                    {
                        TempData["Error"] = "⚠️ Not enough Casual Leave balance.";
                        return RedirectToAction("Manage");
                    }
                    break;
                case "sick leave":
                    if (user.SickLeaveBalance >= days)
                        user.SickLeaveBalance -= days;
                    else
                    {
                        TempData["Error"] = "⚠️ Not enough Sick Leave balance.";
                        return RedirectToAction("Manage");
                    }
                    break;
                case "annual leave":
                    if (user.AnnualLeaveBalance >= days)
                        user.AnnualLeaveBalance -= days;
                    else
                    {
                        TempData["Error"] = "⚠️ Not enough Annual Leave balance.";
                        return RedirectToAction("Manage");
                    }
                    break;
                default:
                    TempData["Error"] = "⚠️ Unknown leave type.";
                    return RedirectToAction("Manage");
            }

            leave.Status = "Approved";
            _context.LeaveRequests.Update(leave);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ {user.FullName}'s {leave.LeaveType} approved. Balance updated.";
            return RedirectToAction("Manage");
        }

        // ✅ ADMIN: Reject leave
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject([FromForm] int id)
        {
            var leave = await _context.LeaveRequests
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
            {
                TempData["Error"] = "❌ Leave request not found.";
                return RedirectToAction("Manage");
            }

            leave.Status = "Rejected";
            _context.LeaveRequests.Update(leave);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"❌ {leave.User?.FullName}'s leave rejected.";
            return RedirectToAction("Manage");
        }

        // ✅ USER: Delete a leave request (any status)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var leave = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == user.Id);

            if (leave == null)
            {
                TempData["Error"] = "❌ Leave request not found.";
                return RedirectToAction("MyLeaves");
            }

            _context.LeaveRequests.Remove(leave);
            await _context.SaveChangesAsync();

            TempData["Message"] = "🗑 Leave request deleted successfully.";
            return RedirectToAction("MyLeaves");
        }


    }


}
