using FinanceApp.Models;
using FinanceApp.Models.ViewModels;

namespace FinanceApp.Services
{
    public interface IVoucherService
    {
        Task<string> GenerateVoucherNumberAsync(string voucherType, int? financialYearId = null);
        Task<bool> PostReceiptVoucherAsync(ReceiptVoucherViewModel model, string userId);
        Task<bool> PostPaymentVoucherAsync(PaymentVoucherViewModel model, string userId);
        Task<bool> PostJournalVoucherAsync(VoucherViewModel model, string userId);
        Task<bool> PostContraVoucherAsync(ContraVoucherViewModel model, string userId);
        Task<bool> ValidateDoubleEntryAsync(List<VoucherLineViewModel> lines);
        Task UpdateLedgerBalanceAsync(int ledgerId, decimal debitAmount, decimal creditAmount, DateTime transactionDate);
    }
}

