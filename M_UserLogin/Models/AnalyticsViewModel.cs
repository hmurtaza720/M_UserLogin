using System.Collections.Generic;

namespace M_UserLogin.Models
{
    public class AnalyticsViewModel
    {
        // Existing analytics
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public List<Users> UsersList { get; set; } = new();

        // New Attendance analytics
        public int TotalAttendanceRecords { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }

        // Attendance trend data (date, count)
        public List<string> AttendanceDates { get; set; } = new();
        public List<int> CheckInCounts { get; set; } = new();
    }
}
