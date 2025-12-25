using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class VendorService : IVendorService
    {
        private readonly ApplicationDbContext _context;

        public VendorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vendor>> GetAllVendorsAsync()
        {
            return await _context.Vendors
                .Include(v => v.Ledger)
                .OrderBy(v => v.VendorName)
                .ToListAsync();
        }

        public async Task<Vendor?> GetVendorByIdAsync(int id)
        {
            return await _context.Vendors
                .Include(v => v.Ledger)
                .FirstOrDefaultAsync(v => v.VendorID == id);
        }

        public async Task<Vendor?> GetVendorByCodeAsync(string code)
        {
            return await _context.Vendors
                .FirstOrDefaultAsync(v => v.VendorCode == code);
        }

        public async Task<bool> CreateVendorAsync(Vendor vendor)
        {
            try
            {
                vendor.CreatedDate = DateTime.Now;
                _context.Vendors.Add(vendor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateVendorAsync(Vendor vendor)
        {
            try
            {
                vendor.ModifiedDate = DateTime.Now;
                _context.Vendors.Update(vendor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteVendorAsync(int id)
        {
            try
            {
                var vendor = await GetVendorByIdAsync(id);
                if (vendor == null) return false;

                // Check if vendor has transactions
                var hasTransactions = await _context.JournalEntries
                    .AnyAsync(e => e.VendorID == id);

                if (hasTransactions)
                {
                    // Soft delete
                    vendor.IsActive = false;
                    await UpdateVendorAsync(vendor);
                }
                else
                {
                    _context.Vendors.Remove(vendor);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> GetVendorBalanceAsync(int vendorId)
        {
            var vendor = await GetVendorByIdAsync(vendorId);
            if (vendor == null) return 0;

            decimal balance = vendor.OpeningBalance;

            // Get all transactions for this vendor
            var transactions = await _context.JournalEntries
                .Include(e => e.JournalEntryLines)
                .Where(e => e.VendorID == vendorId && e.IsPosted && !e.IsCancelled)
                .ToListAsync();

            foreach (var entry in transactions)
            {
                if (entry.JournalEntryLines != null)
                {
                    foreach (var line in entry.JournalEntryLines)
                    {
                        if (line.LedgerID == vendor.LedgerID)
                        {
                            if (line.DebitAmount > 0)
                                balance += line.DebitAmount;
                            if (line.CreditAmount > 0)
                                balance -= line.CreditAmount;
                        }
                    }
                }
            }

            return balance;
        }

        public async Task<decimal> GetVendorOutstandingAsync(int vendorId)
        {
            // Outstanding = Credit balance (amount we owe to vendor)
            var balance = await GetVendorBalanceAsync(vendorId);
            return balance > 0 ? balance : 0; // Only positive (credit) balances are outstanding
        }

        public async Task<List<Vendor>> GetVendorsWithOutstandingAsync()
        {
            var vendors = await GetAllVendorsAsync();
            var vendorsWithOutstanding = new List<Vendor>();

            foreach (var vendor in vendors.Where(v => v.IsActive))
            {
                var outstanding = await GetVendorOutstandingAsync(vendor.VendorID);
                if (outstanding > 0)
                {
                    vendorsWithOutstanding.Add(vendor);
                }
            }

            return vendorsWithOutstanding.OrderByDescending(v => GetVendorOutstandingAsync(v.VendorID).Result).ToList();
        }
    }
}

