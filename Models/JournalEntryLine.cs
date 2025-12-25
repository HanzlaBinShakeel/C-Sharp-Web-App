using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class JournalEntryLine
    {
        [Key]
        public int LineID { get; set; }

        [Required]
        public int EntryID { get; set; }

        [ForeignKey("EntryID")]
        public virtual JournalEntry? JournalEntry { get; set; }

        [Required]
        public int LedgerID { get; set; }

        [ForeignKey("LedgerID")]
        public virtual Ledger? Ledger { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? Sequence { get; set; }
    }
}

