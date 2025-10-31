using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace M_UserLogin.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }

        // 🔐 Secret Code (used for password recovery)
        public string SecretCode { get; set; } = string.Empty;

        // 🗓️ Leave Balances
        [Range(0, 50)]
        public int CasualLeaveBalance { get; set; } = 12; // default yearly quota

        [Range(0, 50)]
        public int SickLeaveBalance { get; set; } = 6; // default yearly quota

        [Range(0, 50)]
        public int AnnualLeaveBalance { get; set; } = 10; // optional extra type

        // 📅 Relationship: A user can have multiple leave requests
        public ICollection<LeaveRequest>? LeaveRequests { get; set; }
    }
}
