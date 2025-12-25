using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByCodeAsync(string code);
        Task<bool> CreateCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<decimal> GetCustomerBalanceAsync(int customerId);
        Task<decimal> GetCustomerOutstandingAsync(int customerId);
        Task<List<Customer>> GetCustomersWithOutstandingAsync();
    }
}

