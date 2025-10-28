using System.Collections.Generic;

namespace M_UserLogin.Models
{
    public class AnalyticsViewModel
    {
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public Dictionary<string, int> RoleDistribution { get; set; } = new();
        public Dictionary<string, int> TopEmailDomains { get; set; } = new();
        public List<Users> UsersList { get; set; } = new();
    }
}
