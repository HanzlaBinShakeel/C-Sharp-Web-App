using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IFinancialYearService
    {
        Task<List<FinancialYear>> GetAllFinancialYearsAsync();
        Task<FinancialYear?> GetFinancialYearByIdAsync(int id);
        Task<FinancialYear?> GetActiveFinancialYearAsync();
        Task<bool> CreateFinancialYearAsync(FinancialYear financialYear);
        Task<bool> UpdateFinancialYearAsync(FinancialYear financialYear);
        Task<bool> SetActiveFinancialYearAsync(int id);
        Task<bool> CloseFinancialYearAsync(int id);
    }
}

