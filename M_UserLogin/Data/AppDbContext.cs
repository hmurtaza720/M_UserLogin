using M_UserLogin.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace M_UserLogin.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options) { }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }


        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        // ✅ Configure Relationship (this line ensures .Include(l => l.User) works)
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LeaveRequest>()
                .HasOne(l => l.User)
                .WithMany() // one user can have many leaves
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
