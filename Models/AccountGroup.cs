using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class AccountGroup
    {
        [Key]
        public int GroupID { get; set; }

        [Required]
        [StringLength(50)]
        public string GroupCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string GroupName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string GroupType { get; set; } = string.Empty; // Asset, Liability, Income, Expense

        public int? ParentGroupID { get; set; }

        [ForeignKey("ParentGroupID")]
        public AccountGroup? ParentGroup { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<AccountGroup>? SubGroups { get; set; }
        public virtual ICollection<Ledger>? Ledgers { get; set; }
    }
}

