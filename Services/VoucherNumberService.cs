using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class VoucherNumberService : IVoucherNumberService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinancialYearService _financialYearService;

        public VoucherNumberService(ApplicationDbContext context, IFinancialYearService financialYearService)
        {
            _context = context;
            _financialYearService = financialYearService;
        }

        public async Task<string> GetNextVoucherNumberAsync(string voucherType, int? financialYearId = null)
        {
            // Get active financial year if not provided
            if (financialYearId == null)
            {
                var activeYear = await _financialYearService.GetActiveFinancialYearAsync();
                if (activeYear == null)
                {
                    // Fallback to simple numbering if no financial year
                    return await GetSimpleVoucherNumberAsync(voucherType);
                }
                financialYearId = activeYear.FinancialYearID;
            }

            var voucherNumber = await GetVoucherNumberAsync(voucherType, financialYearId.Value);
            
            if (voucherNumber == null)
            {
                // Initialize with default prefix
                var prefix = voucherType switch
                {
                    "Receipt" => "RCP",
                    "Payment" => "PAY",
                    "Journal" => "JRN",
                    "Contra" => "CNT",
                    _ => "VCH"
                };

                await InitializeVoucherNumberAsync(voucherType, financialYearId.Value, prefix);
                voucherNumber = await GetVoucherNumberAsync(voucherType, financialYearId.Value);
            }

            if (voucherNumber == null)
            {
                return await GetSimpleVoucherNumberAsync(voucherType);
            }

            // Increment and save
            voucherNumber.CurrentNumber++;
            voucherNumber.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            // Format the number
            var number = voucherNumber.CurrentNumber.ToString("D4");
            var formatted = voucherNumber.Format
                .Replace("{Prefix}", voucherNumber.Prefix)
                .Replace("{Number}", number)
                .Replace("{Suffix}", voucherNumber.Suffix ?? "");

            return formatted;
        }

        private async Task<string> GetSimpleVoucherNumberAsync(string voucherType)
        {
            var prefix = voucherType switch
            {
                "Receipt" => "RCP",
                "Payment" => "PAY",
                "Journal" => "JRN",
                "Contra" => "CNT",
                _ => "VCH"
            };

            var lastEntry = await _context.JournalEntries
                .Where(e => e.VoucherType == voucherType)
                .OrderByDescending(e => e.EntryID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastEntry != null)
            {
                var lastNumber = lastEntry.EntryNumber.Replace(prefix + "-", "");
                if (int.TryParse(lastNumber, out int num))
                {
                    nextNumber = num + 1;
                }
            }

            return $"{prefix}-{nextNumber:D6}";
        }

        public async Task<bool> InitializeVoucherNumberAsync(string voucherType, int financialYearId, string prefix, string? suffix = null)
        {
            try
            {
                var existing = await GetVoucherNumberAsync(voucherType, financialYearId);
                if (existing != null) return true; // Already initialized

                var voucherNumber = new VoucherNumber
                {
                    VoucherType = voucherType,
                    FinancialYearID = financialYearId,
                    Prefix = prefix,
                    Suffix = suffix,
                    CurrentNumber = 0,
                    Format = suffix != null ? "{Prefix}-{Number}-{Suffix}" : "{Prefix}-{Number}",
                    CreatedDate = DateTime.Now
                };

                _context.VoucherNumbers.Add(voucherNumber);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<VoucherNumber?> GetVoucherNumberAsync(string voucherType, int financialYearId)
        {
            return await _context.VoucherNumbers
                .FirstOrDefaultAsync(v => v.VoucherType == voucherType && v.FinancialYearID == financialYearId);
        }

        public async Task<bool> UpdateVoucherNumberAsync(string voucherType, int financialYearId, string prefix, int currentNumber, string? suffix = null)
        {
            try
            {
                var voucherNumber = await GetVoucherNumberAsync(voucherType, financialYearId);
                
                if (voucherNumber == null)
                {
                    // Create new if doesn't exist
                    return await InitializeVoucherNumberAsync(voucherType, financialYearId, prefix, suffix);
                }

                voucherNumber.Prefix = prefix;
                voucherNumber.CurrentNumber = currentNumber;
                if (suffix != null)
                {
                    voucherNumber.Suffix = suffix;
                    voucherNumber.Format = "{Prefix}-{Number}-{Suffix}";
                }
                voucherNumber.ModifiedDate = DateTime.Now;

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

