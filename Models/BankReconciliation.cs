using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class BankReconciliation
    {
        [Key]
        public int ReconciliationID { get; set; }

        [Required]
        public int LedgerID { get; set; } // Bank Account Ledger

        [ForeignKey("LedgerID")]
        public virtual Ledger? Ledger { get; set; }

        [Required]
        public DateTime StatementDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BookBalance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal StatementBalance { get; set; } = 0;

        [StringLength(450)]
        public string? ReconciledBy { get; set; }

        public DateTime? ReconciledDate { get; set; }

        public bool IsReconciled { get; set; } = false;

        // Navigation properties
        public virtual ICollection<BankReconciliationItem>? ReconciliationItems { get; set; }
    }
}

