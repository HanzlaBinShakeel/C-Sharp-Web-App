using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models.ViewModels
{
    public class VoucherViewModel
    {
        public int EntryID { get; set; }

        [Required]
        [Display(Name = "Voucher Date")]
        [DataType(DataType.Date)]
        public DateTime EntryDate { get; set; } = DateTime.Now;

        [Display(Name = "Reference Number")]
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Description/Narration")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string VoucherType { get; set; } = "Journal";

        public List<VoucherLineViewModel> Lines { get; set; } = new List<VoucherLineViewModel>();
    }

    public class VoucherLineViewModel
    {
        public int LineID { get; set; }

        [Required]
        [Display(Name = "Ledger")]
        public int LedgerID { get; set; }

        public string? LedgerName { get; set; }

        [Display(Name = "Debit Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; }

        [Display(Name = "Credit Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }
    }
}

