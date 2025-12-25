using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class LedgerBalance
    {
        [Key]
        public int BalanceID { get; set; }

        [Required]
        public int LedgerID { get; set; }

        [ForeignKey("LedgerID")]
        public virtual Ledger? Ledger { get; set; }

        [Required]
        public DateTime BalanceDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitBalance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditBalance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetBalance { get; set; } = 0;

        [StringLength(10)]
        public string BalanceType { get; set; } = "Debit"; // Debit or Credit
    }
}

