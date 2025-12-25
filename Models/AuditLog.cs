using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogID { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty; // e.g., "JournalEntry", "Ledger", "Vendor"

        [Required]
        public int EntityID { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, View, Post, Cancel

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(450)]
        public string? UserID { get; set; }

        [StringLength(200)]
        public string? UserName { get; set; }

        [StringLength(50)]
        public string? IPAddress { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [Column(TypeName = "text")]
        public string? OldValues { get; set; } // JSON string of old values

        [Column(TypeName = "text")]
        public string? NewValues { get; set; } // JSON string of new values
    }
}

