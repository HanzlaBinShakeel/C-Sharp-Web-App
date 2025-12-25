using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Ledger)
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Ledger)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
        }

        public async Task<Customer?> GetCustomerByCodeAsync(string code)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerCode == code);
        }

        public async Task<bool> CreateCustomerAsync(Customer customer)
        {
            try
            {
                customer.CreatedDate = DateTime.Now;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                customer.ModifiedDate = DateTime.Now;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await GetCustomerByIdAsync(id);
                if (customer == null) return false;

                // Check if customer has transactions
                var hasTransactions = await _context.JournalEntries
                    .AnyAsync(e => e.CustomerID == id);

                if (hasTransactions)
                {
                    // Soft delete
                    customer.IsActive = false;
                    await UpdateCustomerAsync(customer);
                }
                else
                {
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> GetCustomerBalanceAsync(int customerId)
        {
            var customer = await GetCustomerByIdAsync(customerId);
            if (customer == null) return 0;

            decimal balance = customer.OpeningBalance;

            // Get all transactions for this customer
            var transactions = await _context.JournalEntries
                .Include(e => e.JournalEntryLines)
                .Where(e => e.CustomerID == customerId && e.IsPosted && !e.IsCancelled)
                .ToListAsync();

            foreach (var entry in transactions)
            {
                if (entry.JournalEntryLines != null)
                {
                    foreach (var line in entry.JournalEntryLines)
                    {
                        if (line.LedgerID == customer.LedgerID)
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

        public async Task<decimal> GetCustomerOutstandingAsync(int customerId)
        {
            // Outstanding = Debit balance (amount customer owes us)
            var balance = await GetCustomerBalanceAsync(customerId);
            return balance > 0 ? balance : 0; // Only positive (debit) balances are outstanding
        }

        public async Task<List<Customer>> GetCustomersWithOutstandingAsync()
        {
            var customers = await GetAllCustomersAsync();
            var customersWithOutstanding = new List<Customer>();

            foreach (var customer in customers.Where(c => c.IsActive))
            {
                var outstanding = await GetCustomerOutstandingAsync(customer.CustomerID);
                if (outstanding > 0)
                {
                    customersWithOutstanding.Add(customer);
                }
            }

            return customersWithOutstanding.OrderByDescending(c => GetCustomerOutstandingAsync(c.CustomerID).Result).ToList();
        }
    }
}

