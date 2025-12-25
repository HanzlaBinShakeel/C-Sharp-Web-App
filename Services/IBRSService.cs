using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IBRSService
    {
        Task<BankReconciliation?> GetReconciliationAsync(int ledgerId, DateTime statementDate);
        Task<bool> CreateReconciliationAsync(BankReconciliation reconciliation);
        Task<bool> UpdateReconciliationAsync(BankReconciliation reconciliation);
        Task<bool> AddReconciliationItemAsync(int reconciliationId, BankReconciliationItem item);
        Task<bool> MarkItemAsClearedAsync(int itemId);
        Task<BRSReport> GenerateBRSReportAsync(int ledgerId, DateTime statementDate);
    }

    public class BRSReport
    {
        public int LedgerID { get; set; }
        public string BankName { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; }
        public decimal BookBalance { get; set; }
        public List<BRSItem> AddItems { get; set; } = new();
        public List<BRSItem> LessItems { get; set; } = new();
        public decimal ReconciledBalance { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal Difference { get; set; }
    }

    public class BRSItem
    {
        public string ItemType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsCleared { get; set; }
    }
}

