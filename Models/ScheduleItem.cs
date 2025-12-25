using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class ScheduleItem
    {
        [Key]
        public int ItemID { get; set; }

        [Required]
        public int ScheduleID { get; set; }

        [ForeignKey("ScheduleID")]
        public virtual Schedule? Schedule { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ItemCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime? ItemDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

