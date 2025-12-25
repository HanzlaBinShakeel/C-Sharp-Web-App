using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models.ViewModels
{
    public class ReceiptVoucherViewModel
    {
        public int VoucherID { get; set; }

        [Required]
        [Display(Name = "Receipt Date")]
        [DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Received From")]
        public int ReceivedFromLedgerID { get; set; }

        [Required]
        [Display(Name = "Received In")]
        public int ReceivedInLedgerID { get; set; }

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

