using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IVoucherNumberService
    {
        Task<string> GetNextVoucherNumberAsync(string voucherType, int? financialYearId = null);
        Task<bool> InitializeVoucherNumberAsync(string voucherType, int financialYearId, string prefix, string? suffix = null);
        Task<VoucherNumber?> GetVoucherNumberAsync(string voucherType, int financialYearId);
        Task<bool> UpdateVoucherNumberAsync(string voucherType, int financialYearId, string prefix, int currentNumber, string? suffix = null);
    }
}

