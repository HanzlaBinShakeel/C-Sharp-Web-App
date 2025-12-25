using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class JournalEntry
    {
        [Key]
        public int EntryID { get; set; }

        [Required]
        [StringLength(50)]
        public string EntryNumber { get; set; } = string.Empty;

        [Required]
        public DateTime EntryDate { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string VoucherType { get; set; } = "Journal"; // Journal, Receipt, Payment, Contra

        public int? FinancialYearID { get; set; }

        [ForeignKey("FinancialYearID")]
        public virtual FinancialYear? FinancialYear { get; set; }

        public int? VendorID { get; set; } // For AP transactions

        [ForeignKey("VendorID")]
        public virtual Vendor? Vendor { get; set; }

        public int? CustomerID { get; set; } // For AR transactions

        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; } = 0;

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsPosted { get; set; } = false;

        public bool IsCancelled { get; set; } = false;

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        // Navigation properties
        public virtual ICollection<JournalEntryLine>? JournalEntryLines { get; set; }
    }
}

