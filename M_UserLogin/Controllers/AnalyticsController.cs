using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M_UserLogin.Data;
using M_UserLogin.Models;
using System.Linq;
using System.Text.Json;

namespace M_UserLogin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var admins = await _context.UserRoles.CountAsync();

            // ✅ Attendance analytics
            var attendanceData = await _context.AttendanceRecords.ToListAsync();

            int totalRecords = attendanceData.Count;
            int presentCount = attendanceData.Count(a => a.CheckOutTime != null);
            int absentCount = totalUsers - (attendanceData.Select(a => a.UserId).Distinct().Count());

            // ✅ Attendance trend (group by date)
            var groupedData = attendanceData
                .GroupBy(a => a.Date.Date)
                .Select(g => new { Date = g.Key.ToShortDateString(), Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToList();

            var model = new AnalyticsViewModel
            {
                TotalUsers = totalUsers,
                AdminCount = admins,
                TotalAttendanceRecords = totalRecords,
                TotalPresent = presentCount,
                TotalAbsent = absentCount,
                AttendanceDates = groupedData.Select(g => g.Date).ToList(),
                CheckInCounts = groupedData.Select(g => g.Count).ToList(),
                UsersList = await _context.Users.ToListAsync()
            };

            // Role Distribution JSON for pie chart
            var roles = await _context.UserRoles.ToListAsync();
            var roleCounts = roles.GroupBy(r => r.RoleId).ToDictionary(g => g.Key.ToString(), g => g.Count());
            ViewBag.RoleDistributionJson = JsonSerializer.Serialize(roleCounts);

            return View(model);
        }
    }
}
