using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace M_UserLogin.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Leave type is required.")]
        public string LeaveType { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500)]
        public string Reason { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; }
    }
}
