using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models.ViewModels
{
    public class PaymentVoucherViewModel
    {
        public int VoucherID { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Paid To")]
        public int PaidToLedgerID { get; set; }

        [Required]
        [Display(Name = "Paid From")]
        public int PaidFromLedgerID { get; set; }

        [Required]
        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Mode")]
        [StringLength(50)]
        public string PaymentMode { get; set; } = "Cash";

        [Display(Name = "Cheque Number")]
        [StringLength(100)]
        public string? ChequeNumber { get; set; }

        [Display(Name = "Reference Number")]
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Narration")]
        [StringLength(500)]
        public string? Narration { get; set; }
    }
}

