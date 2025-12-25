using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILedgerService _ledgerService;

        public VoucherService(ApplicationDbContext context, ILedgerService ledgerService)
        {
            _context = context;
            _ledgerService = ledgerService;
        }

        public async Task<string> GenerateVoucherNumberAsync(string voucherType, int? financialYearId = null)
        {
            // Try to use VoucherNumberService if available (via dependency injection)
            // For now, fallback to simple numbering
            var prefix = voucherType switch
            {
                "Receipt" => "RCP",
                "Payment" => "PAY",
                "Journal" => "JRN",
                "Contra" => "CTR",
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

        public async Task<bool> ValidateDoubleEntryAsync(List<VoucherLineViewModel> lines)
        {
            if (lines == null || lines.Count < 2)
                return false;

            decimal totalDebit = lines.Sum(l => l.DebitAmount);
            decimal totalCredit = lines.Sum(l => l.CreditAmount);

            // Check if debits equal credits
            if (Math.Abs(totalDebit - totalCredit) > 0.01m)
                return false;

            // Check that each line has either debit OR credit, not both
            foreach (var line in lines)
            {
                if (line.DebitAmount > 0 && line.CreditAmount > 0)
                    return false;
                if (line.DebitAmount == 0 && line.CreditAmount == 0)
                    return false;
            }

            return true;
        }

        public async Task<bool> PostReceiptVoucherAsync(ReceiptVoucherViewModel model, string userId)
        {
            try
            {
                var voucherNumber = await GenerateVoucherNumberAsync("Receipt");

                var entry = new JournalEntry
                {
                    EntryNumber = voucherNumber,
                    EntryDate = model.VoucherDate,
                    ReferenceNumber = model.ReferenceNumber,
                    Description = model.Narration,
                    VoucherType = "Receipt",
                    TotalDebit = model.Amount,
                    TotalCredit = model.Amount,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsPosted = true
                };

                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                // Create debit line (Cash/Bank)
                var debitLine = new JournalEntryLine
                {
                    EntryID = entry.EntryID,
                    LedgerID = model.ReceivedInLedgerID,
                    DebitAmount = model.Amount,
                    CreditAmount = 0,
                    Description = $"Received from {model.ReceivedFromLedgerID}",
                    Sequence = 1
                };

                // Create credit line (Party)
                var creditLine = new JournalEntryLine
                {
                    EntryID = entry.EntryID,
                    LedgerID = model.ReceivedFromLedgerID,
                    DebitAmount = 0,
                    CreditAmount = model.Amount,
                    Description = $"Received in {model.ReceivedInLedgerID}",
                    Sequence = 2
                };

                _context.JournalEntryLines.AddRange(debitLine, creditLine);
                await _context.SaveChangesAsync();

                // Update ledger balances
                await UpdateLedgerBalanceAsync(model.ReceivedInLedgerID, model.Amount, 0, model.VoucherDate);
                await UpdateLedgerBalanceAsync(model.ReceivedFromLedgerID, 0, model.Amount, model.VoucherDate);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostPaymentVoucherAsync(PaymentVoucherViewModel model, string userId)
        {
            try
            {
                var voucherNumber = await GenerateVoucherNumberAsync("Payment");

                var entry = new JournalEntry
                {
                    EntryNumber = voucherNumber,
                    EntryDate = model.VoucherDate,
                    ReferenceNumber = model.ReferenceNumber,
                    Description = model.Narration,
                    VoucherType = "Payment",
                    TotalDebit = model.Amount,
                    TotalCredit = model.Amount,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsPosted = true
                };

                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                // Create debit line (Party)
                var debitLine = new JournalEntryLine
                {
                    EntryID = entry.EntryID,
                    LedgerID = model.PaidToLedgerID,
                    DebitAmount = model.Amount,
                    CreditAmount = 0,
                    Description = $"Paid from {model.PaidFromLedgerID}",
                    Sequence = 1
                };

                // Create credit line (Cash/Bank)
                var creditLine = new JournalEntryLine
                {
                    EntryID = entry.EntryID,
                    LedgerID = model.PaidFromLedgerID,
                    DebitAmount = 0,
                    CreditAmount = model.Amount,
                    Description = $"Paid to {model.PaidToLedgerID}",
                    Sequence = 2
                };

                _context.JournalEntryLines.AddRange(debitLine, creditLine);
                await _context.SaveChangesAsync();

                // Update ledger balances
                await UpdateLedgerBalanceAsync(model.PaidToLedgerID, model.Amount, 0, model.VoucherDate);
                await UpdateLedgerBalanceAsync(model.PaidFromLedgerID, 0, model.Amount, model.VoucherDate);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostJournalVoucherAsync(VoucherViewModel model, string userId)
        {
            try
            {
                // Validate double entry
                if (!await ValidateDoubleEntryAsync(model.Lines))
                    return false;

                var voucherNumber = await GenerateVoucherNumberAsync("Journal");

                var entry = new JournalEntry
                {
                    EntryNumber = voucherNumber,
                    EntryDate = model.EntryDate,
                    ReferenceNumber = model.ReferenceNumber,
                    Description = model.Description,
                    VoucherType = "Journal",
                    TotalDebit = model.Lines.Sum(l => l.DebitAmount),
                    TotalCredit = model.Lines.Sum(l => l.CreditAmount),
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsPosted = true
                };

                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                // Create journal entry lines
                var lines = new List<JournalEntryLine>();
                int sequence = 1;
                foreach (var line in model.Lines)
                {
                    lines.Add(new JournalEntryLine
                    {
                        EntryID = entry.EntryID,
                        LedgerID = line.LedgerID,
                        DebitAmount = line.DebitAmount,
                        CreditAmount = line.CreditAmount,
                        Description = line.Description,
                        Sequence = sequence++
                    });

                    // Update ledger balances
                    await UpdateLedgerBalanceAsync(line.LedgerID, line.DebitAmount, line.CreditAmount, model.EntryDate);
                }

                _context.JournalEntryLines.AddRange(lines);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PostContraVoucherAsync(ContraVoucherViewModel model, string userId)
        {
            try
            {
                var voucherNumber = await GenerateVoucherNumberAsync("Contra");

                var entry = new JournalEntry
                {
                    EntryNumber = voucherNumber,
                    EntryDate = model.VoucherDate,
                    ReferenceNumber = model.ReferenceNumber,
                    Description = model.Narration,
                    VoucherType = "Contra",
                    TotalDebit = model.Amount,
                    TotalCredit = model.Amount,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsPosted = true
                };

                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                // Create debit line (To Account)
                var debitLine = new JournalEntryLine
                {
                    EntryID = entry.EntryID,
                    LedgerID = model.ToLedgerID,
                    DebitAmount = model.Amount,
                    CreditAmount = 0,
                    Description = $"Transferred from {model.FromLedgerID}",
                    Sequence = 1
                };

                // Create credit line (From Account)
                var creditLine = new JournalEntryLine
                {
                    EntryID = entry.EntryID,
                    LedgerID = model.FromLedgerID,
                    DebitAmount = 0,
                    CreditAmount = model.Amount,
                    Description = $"Transferred to {model.ToLedgerID}",
                    Sequence = 2
                };

                _context.JournalEntryLines.AddRange(debitLine, creditLine);
                await _context.SaveChangesAsync();

                // Update ledger balances
                await UpdateLedgerBalanceAsync(model.ToLedgerID, model.Amount, 0, model.VoucherDate);
                await UpdateLedgerBalanceAsync(model.FromLedgerID, 0, model.Amount, model.VoucherDate);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UpdateLedgerBalanceAsync(int ledgerId, decimal debitAmount, decimal creditAmount, DateTime transactionDate)
        {
            var date = transactionDate.Date;

            // Get or create balance for the date
            var balance = await _context.LedgerBalances
                .Where(b => b.LedgerID == ledgerId && b.BalanceDate == date)
                .FirstOrDefaultAsync();

            if (balance == null)
            {
                // Get previous balance
                var previousBalance = await _context.LedgerBalances
                    .Where(b => b.LedgerID == ledgerId && b.BalanceDate < date)
                    .OrderByDescending(b => b.BalanceDate)
                    .FirstOrDefaultAsync();

                var ledger = await _context.Ledgers.FindAsync(ledgerId);
                decimal prevDebit = previousBalance?.DebitBalance ?? (ledger?.OpeningBalanceType == "Debit" ? ledger.OpeningBalance : 0);
                decimal prevCredit = previousBalance?.CreditBalance ?? (ledger?.OpeningBalanceType == "Credit" ? ledger.OpeningBalance : 0);

                balance = new LedgerBalance
                {
                    LedgerID = ledgerId,
                    BalanceDate = date,
                    DebitBalance = prevDebit,
                    CreditBalance = prevCredit,
                    NetBalance = prevDebit - prevCredit,
                    BalanceType = prevDebit >= prevCredit ? "Debit" : "Credit"
                };
                _context.LedgerBalances.Add(balance);
            }

            // Update balances
            balance.DebitBalance += debitAmount;
            balance.CreditBalance += creditAmount;
            balance.NetBalance = balance.DebitBalance - balance.CreditBalance;
            balance.BalanceType = balance.NetBalance >= 0 ? "Debit" : "Credit";

            await _context.SaveChangesAsync();

            // Update all future balances
            var futureBalances = await _context.LedgerBalances
                .Where(b => b.LedgerID == ledgerId && b.BalanceDate > date)
                .OrderBy(b => b.BalanceDate)
                .ToListAsync();

            foreach (var futureBalance in futureBalances)
            {
                futureBalance.DebitBalance += debitAmount;
                futureBalance.CreditBalance += creditAmount;
                futureBalance.NetBalance = futureBalance.DebitBalance - futureBalance.CreditBalance;
                futureBalance.BalanceType = futureBalance.NetBalance >= 0 ? "Debit" : "Credit";
            }

            if (futureBalances.Any())
                await _context.SaveChangesAsync();
        }
    }
}

