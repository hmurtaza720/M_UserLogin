using M_UserLogin.Data;
using M_UserLogin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace M_UserLogin.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public AttendanceController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 🕒 Mark Attendance Page
        public async Task<IActionResult> Mark()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Now.Date;
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Date == today);

            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Now.Date;

            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Date == today);

            if (existing == null)
            {
                var record = new AttendanceRecord
                {
                    UserId = user.Id,
                    Date = today,
                    CheckInTime = DateTime.Now,
                    Status = "Present"
                };
                _context.AttendanceRecords.Add(record);
            }
            else
            {
                TempData["Message"] = "⚠️ You have already checked in today!";
                return RedirectToAction("Mark");
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "✅ Check-in recorded successfully!";
            return RedirectToAction("Mark");
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Now.Date;

            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Date == today);

            if (record == null)
            {
                TempData["Message"] = "⚠️ You must check in before checking out!";
            }
            else if (record.CheckOutTime != null)
            {
                TempData["Message"] = "⚠️ You already checked out today!";
            }
            else
            {
                record.CheckOutTime = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Checked out successfully!";
            }

            return RedirectToAction("Mark");
        }

        // 👁️ View My Attendance History
        public async Task<IActionResult> MyAttendance()
        {
            var user = await _userManager.GetUserAsync(User);
            var records = await _context.AttendanceRecords
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(records);
        }

        // ⚙️ Admin View - Manage Attendance
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var records = await _context.AttendanceRecords
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(records);
        }
    }
}
