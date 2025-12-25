using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class FinancialYear
    {
        [Key]
        public int FinancialYearID { get; set; }

        [Required]
        [StringLength(50)]
        public string YearName { get; set; } = string.Empty; // e.g., "2024-2025"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = false; // Only one active year at a time

        public bool IsClosed { get; set; } = false; // Year is closed and locked

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<JournalEntry>? JournalEntries { get; set; }
    }
}

