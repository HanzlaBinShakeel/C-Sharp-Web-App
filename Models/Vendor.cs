using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class Vendor
    {
        [Key]
        public int VendorID { get; set; }

        [Required]
        [StringLength(50)]
        public string VendorCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string VendorName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? Mobile { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(50)]
        public string? TaxID { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(50)]
        public string? IFSCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0;

        [StringLength(10)]
        public string OpeningBalanceType { get; set; } = "Credit"; // Credit for vendors (AP)

        public int? LedgerID { get; set; } // Link to Ledger if exists

        [ForeignKey("LedgerID")]
        public virtual Ledger? Ledger { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<JournalEntry>? PurchaseEntries { get; set; }
    }
}

