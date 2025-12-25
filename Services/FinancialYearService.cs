using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class FinancialYearService : IFinancialYearService
    {
        private readonly ApplicationDbContext _context;

        public FinancialYearService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FinancialYear>> GetAllFinancialYearsAsync()
        {
            return await _context.FinancialYears
                .OrderByDescending(f => f.StartDate)
                .ToListAsync();
        }

        public async Task<FinancialYear?> GetFinancialYearByIdAsync(int id)
        {
            return await _context.FinancialYears.FindAsync(id);
        }

        public async Task<FinancialYear?> GetActiveFinancialYearAsync()
        {
            return await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.IsActive && !f.IsClosed);
        }

        public async Task<bool> CreateFinancialYearAsync(FinancialYear financialYear)
        {
            try
            {
                financialYear.CreatedDate = DateTime.Now;
                
                // If this is set as active, deactivate others
                if (financialYear.IsActive)
                {
                    await SetActiveFinancialYearAsync(financialYear.FinancialYearID);
                }

                _context.FinancialYears.Add(financialYear);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateFinancialYearAsync(FinancialYear financialYear)
        {
            try
            {
                _context.FinancialYears.Update(financialYear);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetActiveFinancialYearAsync(int id)
        {
            try
            {
                // Deactivate all years
                var allYears = await _context.FinancialYears.ToListAsync();
                foreach (var year in allYears)
                {
                    year.IsActive = false;
                }

                // Activate the selected year
                var selectedYear = await GetFinancialYearByIdAsync(id);
                if (selectedYear != null)
                {
                    selectedYear.IsActive = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CloseFinancialYearAsync(int id)
        {
            try
            {
                var year = await GetFinancialYearByIdAsync(id);
                if (year == null) return false;

                year.IsClosed = true;
                year.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

