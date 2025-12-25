using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models.ViewModels
{
    public class ContraVoucherViewModel
    {
        public int VoucherID { get; set; }

        [Required]
        [Display(Name = "Contra Date")]
        [DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "From Account")]
        public int FromLedgerID { get; set; }

        [Required]
        [Display(Name = "To Account")]
        public int ToLedgerID { get; set; }

        [Required]
        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Mode")]
        [StringLength(50)]
        public string PaymentMode { get; set; } = "Cash";

        [Display(Name = "Reference Number")]
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Narration")]
        [StringLength(500)]
        public string? Narration { get; set; }
    }
}

