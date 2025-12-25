using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class BRSService : IBRSService
    {
        private readonly ApplicationDbContext _context;

        public BRSService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BankReconciliation?> GetReconciliationAsync(int ledgerId, DateTime statementDate)
        {
            return await _context.BankReconciliations
                .Include(r => r.ReconciliationItems)
                .FirstOrDefaultAsync(r => r.LedgerID == ledgerId && r.StatementDate == statementDate);
        }

        public async Task<bool> CreateReconciliationAsync(BankReconciliation reconciliation)
        {
            try
            {
                _context.BankReconciliations.Add(reconciliation);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateReconciliationAsync(BankReconciliation reconciliation)
        {
            try
            {
                var existing = await _context.BankReconciliations.FindAsync(reconciliation.ReconciliationID);
                if (existing == null)
                    return false;

                existing.BookBalance = reconciliation.BookBalance;
                existing.StatementBalance = reconciliation.StatementBalance;
                existing.IsReconciled = reconciliation.IsReconciled;
                existing.ReconciledBy = reconciliation.ReconciledBy;
                existing.ReconciledDate = reconciliation.ReconciledDate;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddReconciliationItemAsync(int reconciliationId, BankReconciliationItem item)
        {
            try
            {
                item.ReconciliationID = reconciliationId;
                _context.BankReconciliationItems.Add(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkItemAsClearedAsync(int itemId)
        {
            try
            {
                var item = await _context.BankReconciliationItems.FindAsync(itemId);
                if (item == null)
                    return false;

                item.IsCleared = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<BRSReport> GenerateBRSReportAsync(int ledgerId, DateTime statementDate)
        {
            var ledger = await _context.Ledgers.FindAsync(ledgerId);
            if (ledger == null)
                return new BRSReport();

            var reconciliation = await GetReconciliationAsync(ledgerId, statementDate);
            var report = new BRSReport
            {
                LedgerID = ledgerId,
                BankName = ledger.LedgerName,
                StatementDate = statementDate,
                BookBalance = reconciliation?.BookBalance ?? 0,
                StatementBalance = reconciliation?.StatementBalance ?? 0
            };

            if (reconciliation != null && reconciliation.ReconciliationItems != null)
            {
                foreach (var item in reconciliation.ReconciliationItems)
                {
                    var brsItem = new BRSItem
                    {
                        ItemType = item.ItemType,
                        Description = item.Description ?? "",
                        ReferenceNumber = item.ReferenceNumber ?? "",
                        Amount = item.Amount,
                        IsCleared = item.IsCleared
                    };

                    // Categorize items
                    if (item.ItemType.Contains("Deposit") || item.ItemType.Contains("Interest"))
                    {
                        report.AddItems.Add(brsItem);
                        report.ReconciledBalance += item.Amount;
                    }
                    else if (item.ItemType.Contains("Issued") || item.ItemType.Contains("Charges"))
                    {
                        report.LessItems.Add(brsItem);
                        report.ReconciledBalance -= item.Amount;
                    }
                }
            }

            report.ReconciledBalance = report.BookBalance + report.AddItems.Sum(i => i.Amount) - report.LessItems.Sum(i => i.Amount);
            report.Difference = report.ReconciledBalance - report.StatementBalance;

            return report;
        }
    }
}

