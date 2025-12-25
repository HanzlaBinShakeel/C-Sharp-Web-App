using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class VoucherNumber
    {
        [Key]
        public int VoucherNumberID { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherType { get; set; } = string.Empty; // Receipt, Payment, Journal, Contra

        [Required]
        public int FinancialYearID { get; set; }

        [ForeignKey("FinancialYearID")]
        public virtual FinancialYear? FinancialYear { get; set; }

        [Required]
        [StringLength(20)]
        public string Prefix { get; set; } = string.Empty; // e.g., "RCP", "PAY", "JRN", "CNT"

        [Required]
        public int CurrentNumber { get; set; } = 0;

        [StringLength(20)]
        public string? Suffix { get; set; } // Optional suffix

        [StringLength(50)]
        public string Format { get; set; } = "{Prefix}-{Number:0000}"; // Format template

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        // Unique constraint on VoucherType + FinancialYearID
    }
}

