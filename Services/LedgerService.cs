using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class LedgerService : ILedgerService
    {
        private readonly ApplicationDbContext _context;

        public LedgerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ledger>> GetAllLedgersAsync()
        {
            return await _context.Ledgers
                .Include(l => l.Group)
                .Where(l => l.IsActive)
                .OrderBy(l => l.LedgerCode)
                .ToListAsync();
        }

        public async Task<Ledger?> GetLedgerByIdAsync(int id)
        {
            return await _context.Ledgers
                .Include(l => l.Group)
                .FirstOrDefaultAsync(l => l.LedgerID == id);
        }

        public async Task<Ledger?> GetLedgerByCodeAsync(string code)
        {
            return await _context.Ledgers
                .FirstOrDefaultAsync(l => l.LedgerCode == code);
        }

        public async Task<bool> CreateLedgerAsync(Ledger ledger)
        {
            try
            {
                // Check if code already exists
                var existing = await GetLedgerByCodeAsync(ledger.LedgerCode);
                if (existing != null)
                    return false;

                ledger.CreatedDate = DateTime.Now;
                _context.Ledgers.Add(ledger);
                await _context.SaveChangesAsync();

                // Create initial ledger balance
                var balance = new LedgerBalance
                {
                    LedgerID = ledger.LedgerID,
                    BalanceDate = DateTime.Now.Date,
                    DebitBalance = ledger.OpeningBalanceType == "Debit" ? ledger.OpeningBalance : 0,
                    CreditBalance = ledger.OpeningBalanceType == "Credit" ? ledger.OpeningBalance : 0,
                    NetBalance = ledger.OpeningBalanceType == "Debit" ? ledger.OpeningBalance : -ledger.OpeningBalance,
                    BalanceType = ledger.OpeningBalanceType
                };
                _context.LedgerBalances.Add(balance);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLedgerAsync(Ledger ledger)
        {
            try
            {
                var existing = await GetLedgerByIdAsync(ledger.LedgerID);
                if (existing == null)
                    return false;

                // Check if code is being changed and if new code already exists
                if (existing.LedgerCode != ledger.LedgerCode)
                {
                    var codeExists = await GetLedgerByCodeAsync(ledger.LedgerCode);
                    if (codeExists != null)
                        return false;
                }

                existing.LedgerCode = ledger.LedgerCode;
                existing.LedgerName = ledger.LedgerName;
                existing.GroupID = ledger.GroupID;
                existing.Address = ledger.Address;
                existing.ContactInfo = ledger.ContactInfo;
                existing.IsActive = ledger.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteLedgerAsync(int id)
        {
            try
            {
                var ledger = await GetLedgerByIdAsync(id);
                if (ledger == null)
                    return false;

                // Check if ledger has transactions
                var hasTransactions = await _context.JournalEntryLines.AnyAsync(j => j.LedgerID == id);
                if (hasTransactions)
                    return false; // Cannot delete ledger with transactions

                ledger.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Ledger>> GetLedgersByGroupAsync(int groupId)
        {
            return await _context.Ledgers
                .Where(l => l.GroupID == groupId && l.IsActive)
                .OrderBy(l => l.LedgerCode)
                .ToListAsync();
        }

        public async Task<decimal> GetLedgerBalanceAsync(int ledgerId, DateTime? asOfDate = null)
        {
            var date = asOfDate ?? DateTime.Now.Date;
            
            var latestBalance = await _context.LedgerBalances
                .Where(b => b.LedgerID == ledgerId && b.BalanceDate <= date)
                .OrderByDescending(b => b.BalanceDate)
                .FirstOrDefaultAsync();

            if (latestBalance == null)
            {
                var ledger = await GetLedgerByIdAsync(ledgerId);
                return ledger?.OpeningBalance ?? 0;
            }

            return latestBalance.NetBalance;
        }
    }
}

