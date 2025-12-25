using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IVendorService
    {
        Task<List<Vendor>> GetAllVendorsAsync();
        Task<Vendor?> GetVendorByIdAsync(int id);
        Task<Vendor?> GetVendorByCodeAsync(string code);
        Task<bool> CreateVendorAsync(Vendor vendor);
        Task<bool> UpdateVendorAsync(Vendor vendor);
        Task<bool> DeleteVendorAsync(int id);
        Task<decimal> GetVendorBalanceAsync(int vendorId);
        Task<decimal> GetVendorOutstandingAsync(int vendorId);
        Task<List<Vendor>> GetVendorsWithOutstandingAsync();
    }
}

