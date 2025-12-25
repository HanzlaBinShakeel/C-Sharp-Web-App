using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class Schedule
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        [StringLength(50)]
        public string ScheduleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ScheduleType { get; set; } // Fixed Assets, Investments, Loans, etc.

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? LinkedLedgerID { get; set; }

        [ForeignKey("LinkedLedgerID")]
        public virtual Ledger? LinkedLedger { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ScheduleItem>? ScheduleItems { get; set; }
    }
}

