using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class Ledger
    {
        [Key]
        public int LedgerID { get; set; }

        [Required]
        [StringLength(50)]
        public string LedgerCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LedgerName { get; set; } = string.Empty;

        [Required]
        public int GroupID { get; set; }

        [ForeignKey("GroupID")]
        public virtual AccountGroup? Group { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0;

        [StringLength(10)]
        public string OpeningBalanceType { get; set; } = "Debit"; // Debit or Credit

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? ContactInfo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<JournalEntryLine>? JournalEntryLines { get; set; }
        public virtual ICollection<LedgerBalance>? LedgerBalances { get; set; }
    }
}

