using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class BankReconciliationItem
    {
        [Key]
        public int ItemID { get; set; }

        [Required]
        public int ReconciliationID { get; set; }

        [ForeignKey("ReconciliationID")]
        public virtual BankReconciliation? BankReconciliation { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemType { get; set; } = string.Empty; // Cheque Issued, Cheque Deposited, Bank Charges, Interest, etc.

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Cheque number, etc.

        public DateTime? ItemDate { get; set; }

        public bool IsCleared { get; set; } = false;
    }
}

