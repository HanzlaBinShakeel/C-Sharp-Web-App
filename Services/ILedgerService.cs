using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface ILedgerService
    {
        Task<List<Ledger>> GetAllLedgersAsync();
        Task<Ledger?> GetLedgerByIdAsync(int id);
        Task<Ledger?> GetLedgerByCodeAsync(string code);
        Task<bool> CreateLedgerAsync(Ledger ledger);
        Task<bool> UpdateLedgerAsync(Ledger ledger);
        Task<bool> DeleteLedgerAsync(int id);
        Task<List<Ledger>> GetLedgersByGroupAsync(int groupId);
        Task<decimal> GetLedgerBalanceAsync(int ledgerId, DateTime? asOfDate = null);
    }
}

