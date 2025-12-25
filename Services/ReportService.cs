using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;

namespace FinanceApp.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILedgerService _ledgerService;

        public ReportService(ApplicationDbContext context, ILedgerService ledgerService)
        {
            _context = context;
            _ledgerService = ledgerService;
            QuestPDF.Settings.License = LicenseType.Community;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<BalanceSheetReport> GenerateBalanceSheetAsync(DateTime asOfDate)
        {
            var report = new BalanceSheetReport { AsOfDate = asOfDate };

            // Get all asset groups
            var assetGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Asset" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in assetGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, asOfDate);
                    if (balance != 0)
                    {
                        report.Assets.Add(new BalanceSheetItem
                        {
                            GroupName = group.GroupName,
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalAssets += Math.Abs(balance);
                    }
                }
            }

            // Get all liability groups
            var liabilityGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Liability" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in liabilityGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, asOfDate);
                    if (balance != 0)
                    {
                        report.Liabilities.Add(new BalanceSheetItem
                        {
                            GroupName = group.GroupName,
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalLiabilities += Math.Abs(balance);
                    }
                }
            }

            // Get all equity groups
            var equityGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Equity" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in equityGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, asOfDate);
                    if (balance != 0)
                    {
                        report.Equity.Add(new BalanceSheetItem
                        {
                            GroupName = group.GroupName,
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalEquity += Math.Abs(balance);
                    }
                }
            }

            // Calculate retained earnings (Revenue - Expenses)
            var revenueBalance = await GetGroupTotalBalanceAsync("Income", asOfDate);
            var expenseBalance = await GetGroupTotalBalanceAsync("Expense", asOfDate);
            var retainedEarnings = revenueBalance - expenseBalance;

            if (retainedEarnings != 0)
            {
                report.Equity.Add(new BalanceSheetItem
                {
                    GroupName = "Retained Earnings",
                    LedgerName = "Retained Earnings",
                    Amount = Math.Abs(retainedEarnings)
                });
                report.TotalEquity += Math.Abs(retainedEarnings);
            }

            return report;
        }

        public async Task<TrialBalanceReport> GenerateTrialBalanceAsync(DateTime asOfDate)
        {
            var report = new TrialBalanceReport { AsOfDate = asOfDate };

            var ledgers = await _context.Ledgers
                .Where(l => l.IsActive)
                .Include(l => l.Group)
                .ToListAsync();

            foreach (var ledger in ledgers)
            {
                var balance = await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, asOfDate);
                if (balance != 0)
                {
                    var item = new TrialBalanceItem
                    {
                        LedgerCode = ledger.LedgerCode,
                        LedgerName = ledger.LedgerName,
                        DebitBalance = balance > 0 ? balance : 0,
                        CreditBalance = balance < 0 ? Math.Abs(balance) : 0
                    };
                    report.Items.Add(item);
                    report.TotalDebit += item.DebitBalance;
                    report.TotalCredit += item.CreditBalance;
                }
            }

            return report;
        }

        public async Task<ScheduleReport> GenerateScheduleReportAsync(int scheduleId, DateTime asOfDate)
        {
            var schedule = await _context.Schedules
                .Include(s => s.ScheduleItems)
                .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);

            if (schedule == null)
                return new ScheduleReport();

            var report = new ScheduleReport
            {
                ScheduleID = schedule.ScheduleID,
                ScheduleName = schedule.ScheduleName,
                AsOfDate = asOfDate,
                Items = schedule.ScheduleItems?.ToList() ?? new List<ScheduleItem>(),
                TotalAmount = schedule.ScheduleItems?.Sum(i => i.Amount) ?? 0
            };

            return report;
        }

        public async Task<CashbookReport> GenerateDoubleColumnCashbookAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new CashbookReport { FromDate = fromDate, ToDate = toDate };

            // Get Cash and Bank ledgers
            var cashLedger = await _context.Ledgers.FirstOrDefaultAsync(l => l.LedgerName.Contains("Cash") && l.IsActive);
            var bankLedger = await _context.Ledgers.FirstOrDefaultAsync(l => l.LedgerName.Contains("Bank") && l.IsActive);

            if (cashLedger == null || bankLedger == null)
                return report;

            report.OpeningCashBalance = await _ledgerService.GetLedgerBalanceAsync(cashLedger.LedgerID, fromDate.AddDays(-1));
            report.OpeningBankBalance = await _ledgerService.GetLedgerBalanceAsync(bankLedger.LedgerID, fromDate.AddDays(-1));

            // Get all transactions for cash and bank
            var transactions = await _context.JournalEntryLines
                .Where(j => (j.LedgerID == cashLedger.LedgerID || j.LedgerID == bankLedger.LedgerID)
                    && j.JournalEntry.EntryDate >= fromDate
                    && j.JournalEntry.EntryDate <= toDate
                    && j.JournalEntry.IsPosted)
                .Include(j => j.JournalEntry)
                .Include(j => j.Ledger)
                .OrderBy(j => j.JournalEntry.EntryDate)
                .ToListAsync();

            decimal cashBalance = report.OpeningCashBalance;
            decimal bankBalance = report.OpeningBankBalance;

            foreach (var trans in transactions)
            {
                var isCash = trans.LedgerID == cashLedger.LedgerID;
                var receipt = trans.DebitAmount;
                var payment = trans.CreditAmount;

                if (isCash)
                {
                    cashBalance += receipt - payment;
                }
                else
                {
                    bankBalance += receipt - payment;
                }

                report.Transactions.Add(new CashbookTransaction
                {
                    Date = trans.JournalEntry.EntryDate,
                    VoucherNumber = trans.JournalEntry.EntryNumber,
                    Particulars = trans.Description ?? trans.JournalEntry.Description ?? "",
                    CashReceipt = isCash ? receipt : 0,
                    CashPayment = isCash ? payment : 0,
                    BankReceipt = !isCash ? receipt : 0,
                    BankPayment = !isCash ? payment : 0,
                    CashBalance = cashBalance,
                    BankBalance = bankBalance
                });
            }

            report.ClosingCashBalance = cashBalance;
            report.ClosingBankBalance = bankBalance;

            return report;
        }

        public async Task<ReceiptPaymentReport> GenerateReceiptPaymentReportAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new ReceiptPaymentReport { FromDate = fromDate, ToDate = toDate };

            // Get receipt vouchers
            var receipts = await _context.JournalEntries
                .Where(e => e.VoucherType == "Receipt"
                    && e.EntryDate >= fromDate
                    && e.EntryDate <= toDate
                    && e.IsPosted)
                .Include(e => e.JournalEntryLines)
                .ThenInclude(l => l.Ledger)
                .ToListAsync();

            foreach (var receipt in receipts)
            {
                var creditLine = receipt.JournalEntryLines.FirstOrDefault(l => l.CreditAmount > 0);
                if (creditLine != null)
                {
                    report.Receipts.Add(new ReceiptPaymentItem
                    {
                        LedgerName = creditLine.Ledger?.LedgerName ?? "",
                        Amount = creditLine.CreditAmount
                    });
                    report.TotalReceipts += creditLine.CreditAmount;
                }
            }

            // Get payment vouchers
            var payments = await _context.JournalEntries
                .Where(e => e.VoucherType == "Payment"
                    && e.EntryDate >= fromDate
                    && e.EntryDate <= toDate
                    && e.IsPosted)
                .Include(e => e.JournalEntryLines)
                .ThenInclude(l => l.Ledger)
                .ToListAsync();

            foreach (var payment in payments)
            {
                var debitLine = payment.JournalEntryLines.FirstOrDefault(l => l.DebitAmount > 0);
                if (debitLine != null)
                {
                    report.Payments.Add(new ReceiptPaymentItem
                    {
                        LedgerName = debitLine.Ledger?.LedgerName ?? "",
                        Amount = debitLine.DebitAmount
                    });
                    report.TotalPayments += debitLine.DebitAmount;
                }
            }

            report.NetCashFlow = report.TotalReceipts - report.TotalPayments;

            return report;
        }

        public async Task<IncomeExpenditureReport> GenerateIncomeExpenditureReportAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new IncomeExpenditureReport { FromDate = fromDate, ToDate = toDate };

            // Get income ledgers
            var incomeGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Income" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in incomeGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await GetLedgerPeriodBalanceAsync(ledger.LedgerID, fromDate, toDate);
                    if (balance != 0)
                    {
                        report.Income.Add(new IncomeExpenditureItem
                        {
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalIncome += Math.Abs(balance);
                    }
                }
            }

            // Get expense ledgers
            var expenseGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Expense" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in expenseGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await GetLedgerPeriodBalanceAsync(ledger.LedgerID, fromDate, toDate);
                    if (balance != 0)
                    {
                        report.Expenditure.Add(new IncomeExpenditureItem
                        {
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalExpenditure += Math.Abs(balance);
                    }
                }
            }

            report.SurplusDeficit = report.TotalIncome - report.TotalExpenditure;

            return report;
        }

        public async Task<LedgerReconciliationReport> GenerateLedgerReconciliationAsync(int ledgerId, DateTime fromDate, DateTime toDate)
        {
            var ledger = await _context.Ledgers.FindAsync(ledgerId);
            if (ledger == null)
                return new LedgerReconciliationReport();

            var report = new LedgerReconciliationReport
            {
                LedgerID = ledgerId,
                LedgerName = ledger.LedgerName,
                FromDate = fromDate,
                ToDate = toDate
            };

            report.OpeningBalance = await _ledgerService.GetLedgerBalanceAsync(ledgerId, fromDate.AddDays(-1));

            var transactions = await _context.JournalEntryLines
                .Where(j => j.LedgerID == ledgerId
                    && j.JournalEntry.EntryDate >= fromDate
                    && j.JournalEntry.EntryDate <= toDate
                    && j.JournalEntry.IsPosted)
                .Include(j => j.JournalEntry)
                .OrderBy(j => j.JournalEntry.EntryDate)
                .ToListAsync();

            decimal runningBalance = report.OpeningBalance;

            foreach (var trans in transactions)
            {
                runningBalance += trans.DebitAmount - trans.CreditAmount;

                report.Transactions.Add(new LedgerTransaction
                {
                    Date = trans.JournalEntry.EntryDate,
                    VoucherNumber = trans.JournalEntry.EntryNumber,
                    Particulars = trans.Description ?? trans.JournalEntry.Description ?? "",
                    Debit = trans.DebitAmount,
                    Credit = trans.CreditAmount,
                    Balance = runningBalance
                });
            }

            report.ClosingBalance = runningBalance;

            return report;
        }

        private async Task<decimal> GetGroupTotalBalanceAsync(string groupType, DateTime asOfDate)
        {
            var groups = await _context.AccountGroups
                .Where(g => g.GroupType == groupType && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            decimal total = 0;
            foreach (var group in groups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    total += await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, asOfDate);
                }
            }

            return total;
        }

        private async Task<decimal> GetLedgerPeriodBalanceAsync(int ledgerId, DateTime fromDate, DateTime toDate)
        {
            var transactions = await _context.JournalEntryLines
                .Where(j => j.LedgerID == ledgerId
                    && j.JournalEntry.EntryDate >= fromDate
                    && j.JournalEntry.EntryDate <= toDate
                    && j.JournalEntry.IsPosted)
                .ToListAsync();

            return transactions.Sum(t => t.DebitAmount - t.CreditAmount);
        }

        public byte[] GeneratePDFReport(object reportData, string reportType)
        {
            try
            {
                using var stream = new MemoryStream();
                
                if (reportType == "BalanceSheet" && reportData is BalanceSheetReport bsReport)
                {
                    var document = QuestPDF.Fluent.Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(QuestPDF.Helpers.PageSizes.A4);
                            page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                            
                            page.Header().Text($"Balance Sheet as on {bsReport.AsOfDate:dd/MM/yyyy}").FontSize(16).Bold();
                            
                            page.Content().Column(column =>
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("ASSETS").Bold().FontSize(12);
                                        foreach (var asset in bsReport.Assets)
                                        {
                                            col.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text(asset.LedgerName);
                                                r.ConstantItem(100).AlignRight().Text(asset.Amount.ToString("C"));
                                            });
                                        }
                                        col.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text("TOTAL ASSETS").Bold();
                                            r.ConstantItem(100).AlignRight().Text(bsReport.TotalAssets.ToString("C")).Bold();
                                        });
                                    });
                                    
                                    row.ConstantItem(20);
                                    
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("LIABILITIES & EQUITY").Bold().FontSize(12);
                                        foreach (var liability in bsReport.Liabilities)
                                        {
                                            col.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text(liability.LedgerName);
                                                r.ConstantItem(100).AlignRight().Text(liability.Amount.ToString("C"));
                                            });
                                        }
                                        foreach (var equity in bsReport.Equity)
                                        {
                                            col.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text(equity.LedgerName);
                                                r.ConstantItem(100).AlignRight().Text(equity.Amount.ToString("C"));
                                            });
                                        }
                                        col.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text("TOTAL LIABILITIES & EQUITY").Bold();
                                            r.ConstantItem(100).AlignRight().Text((bsReport.TotalLiabilities + bsReport.TotalEquity).ToString("C")).Bold();
                                        });
                                    });
                                });
                            });
                        });
                    });
                    
                    document.GeneratePdf(stream);
                }
                else if (reportType == "TrialBalance" && reportData is TrialBalanceReport tbReport)
                {
                    var document = QuestPDF.Fluent.Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(QuestPDF.Helpers.PageSizes.A4);
                            page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                            
                            page.Header().Text($"Trial Balance as on {tbReport.AsOfDate:dd/MM/yyyy}").FontSize(16).Bold();
                            
                            page.Content().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                
                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").Bold();
                                    header.Cell().Text("Ledger Name").Bold();
                                    header.Cell().AlignRight().Text("Debit").Bold();
                                    header.Cell().AlignRight().Text("Credit").Bold();
                                });
                                
                                foreach (var item in tbReport.Items)
                                {
                                    table.Cell().Text(item.LedgerCode);
                                    table.Cell().Text(item.LedgerName);
                                    table.Cell().AlignRight().Text(item.DebitBalance.ToString("C"));
                                    table.Cell().AlignRight().Text(item.CreditBalance.ToString("C"));
                                }
                                
                                table.Cell().Text("TOTAL").Bold();
                                table.Cell().Text("");
                                table.Cell().AlignRight().Text(tbReport.TotalDebit.ToString("C")).Bold();
                                table.Cell().AlignRight().Text(tbReport.TotalCredit.ToString("C")).Bold();
                            });
                        });
                    });
                    
                    document.GeneratePdf(stream);
                }
                
                return stream.ToArray();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public byte[] GenerateExcelReport(object reportData, string reportType)
        {
            try
            {
                using var package = new OfficeOpenXml.ExcelPackage();
                
                if (reportType == "BalanceSheet" && reportData is BalanceSheetReport bsReport)
                {
                    var worksheet = package.Workbook.Worksheets.Add("Balance Sheet");
                    
                    worksheet.Cells[1, 1].Value = $"Balance Sheet as on {bsReport.AsOfDate:dd/MM/yyyy}";
                    worksheet.Cells[1, 1, 1, 4].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    int row = 3;
                    worksheet.Cells[row, 1].Value = "ASSETS";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    row++;
                    
                    foreach (var asset in bsReport.Assets)
                    {
                        worksheet.Cells[row, 1].Value = asset.LedgerName;
                        worksheet.Cells[row, 2].Value = asset.Amount;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }
                    
                    worksheet.Cells[row, 1].Value = "TOTAL ASSETS";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 2].Value = bsReport.TotalAssets;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 2].Style.Font.Bold = true;
                    
                    row = 3;
                    worksheet.Cells[row, 3].Value = "LIABILITIES & EQUITY";
                    worksheet.Cells[row, 3].Style.Font.Bold = true;
                    row++;
                    
                    foreach (var liability in bsReport.Liabilities)
                    {
                        worksheet.Cells[row, 3].Value = liability.LedgerName;
                        worksheet.Cells[row, 4].Value = liability.Amount;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }
                    
                    foreach (var equity in bsReport.Equity)
                    {
                        worksheet.Cells[row, 3].Value = equity.LedgerName;
                        worksheet.Cells[row, 4].Value = equity.Amount;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }
                    
                    worksheet.Cells[row, 3].Value = "TOTAL LIABILITIES & EQUITY";
                    worksheet.Cells[row, 3].Style.Font.Bold = true;
                    worksheet.Cells[row, 4].Value = bsReport.TotalLiabilities + bsReport.TotalEquity;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 4].Style.Font.Bold = true;
                }
                else if (reportType == "TrialBalance" && reportData is TrialBalanceReport tbReport)
                {
                    var worksheet = package.Workbook.Worksheets.Add("Trial Balance");
                    
                    worksheet.Cells[1, 1].Value = $"Trial Balance as on {tbReport.AsOfDate:dd/MM/yyyy}";
                    worksheet.Cells[1, 1, 1, 4].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    int row = 3;
                    worksheet.Cells[row, 1].Value = "Code";
                    worksheet.Cells[row, 2].Value = "Ledger Name";
                    worksheet.Cells[row, 3].Value = "Debit";
                    worksheet.Cells[row, 4].Value = "Credit";
                    worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;
                    row++;
                    
                    foreach (var item in tbReport.Items)
                    {
                        worksheet.Cells[row, 1].Value = item.LedgerCode;
                        worksheet.Cells[row, 2].Value = item.LedgerName;
                        worksheet.Cells[row, 3].Value = item.DebitBalance;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 4].Value = item.CreditBalance;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }
                    
                    worksheet.Cells[row, 1].Value = "TOTAL";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 3].Value = tbReport.TotalDebit;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 3].Style.Font.Bold = true;
                    worksheet.Cells[row, 4].Value = tbReport.TotalCredit;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 4].Style.Font.Bold = true;
                }
                
                return package.GetAsByteArray();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public async Task<APAgingReport> GenerateAPAgingReportAsync(DateTime asOfDate)
        {
            var report = new APAgingReport { AsOfDate = asOfDate };
            var vendors = await _context.Vendors
                .Where(v => v.IsActive)
                .Include(v => v.Ledger)
                .ToListAsync();

            foreach (var vendor in vendors)
            {
                if (vendor.LedgerID == null) continue;

                var transactions = await _context.JournalEntryLines
                    .Include(l => l.JournalEntry)
                    .Where(l => l.LedgerID == vendor.LedgerID && 
                                l.JournalEntry != null &&
                                l.JournalEntry.EntryDate <= asOfDate &&
                                l.JournalEntry.IsPosted &&
                                !l.JournalEntry.IsCancelled)
                    .OrderBy(l => l.JournalEntry!.EntryDate)
                    .ToListAsync();

                decimal runningBalance = vendor.OpeningBalance;
                var agingItem = new APAgingItem
                {
                    VendorCode = vendor.VendorCode,
                    VendorName = vendor.VendorName
                };

                foreach (var trans in transactions)
                {
                    if (trans.JournalEntry == null) continue;
                    runningBalance += trans.CreditAmount - trans.DebitAmount;
                    
                    var daysOld = (asOfDate - trans.JournalEntry.EntryDate).Days;
                    var amount = Math.Abs(trans.CreditAmount - trans.DebitAmount);
                    
                    if (runningBalance > 0) // We owe money (credit balance)
                    {
                        if (daysOld <= 30)
                            agingItem.Current += amount;
                        else if (daysOld <= 60)
                            agingItem.Days30 += amount;
                        else if (daysOld <= 90)
                            agingItem.Days60 += amount;
                        else
                            agingItem.Days90 += amount;
                    }
                }

                agingItem.TotalOutstanding = agingItem.Current + agingItem.Days30 + agingItem.Days60 + agingItem.Days90;
                
                if (agingItem.TotalOutstanding > 0)
                {
                    report.Items.Add(agingItem);
                    report.TotalCurrent += agingItem.Current;
                    report.Total30Days += agingItem.Days30;
                    report.Total60Days += agingItem.Days60;
                    report.Total90Days += agingItem.Days90;
                }
            }

            report.GrandTotal = report.TotalCurrent + report.Total30Days + report.Total60Days + report.Total90Days;
            return report;
        }

        public async Task<ARAgingReport> GenerateARAgingReportAsync(DateTime asOfDate)
        {
            var report = new ARAgingReport { AsOfDate = asOfDate };
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.Ledger)
                .ToListAsync();

            foreach (var customer in customers)
            {
                if (customer.LedgerID == null) continue;

                var transactions = await _context.JournalEntryLines
                    .Include(l => l.JournalEntry)
                    .Where(l => l.LedgerID == customer.LedgerID && 
                                l.JournalEntry != null &&
                                l.JournalEntry.EntryDate <= asOfDate &&
                                l.JournalEntry.IsPosted &&
                                !l.JournalEntry.IsCancelled)
                    .OrderBy(l => l.JournalEntry!.EntryDate)
                    .ToListAsync();

                decimal runningBalance = customer.OpeningBalance;
                var agingItem = new ARAgingItem
                {
                    CustomerCode = customer.CustomerCode,
                    CustomerName = customer.CustomerName
                };

                foreach (var trans in transactions)
                {
                    if (trans.JournalEntry == null) continue;
                    runningBalance += trans.DebitAmount - trans.CreditAmount;
                    
                    var daysOld = (asOfDate - trans.JournalEntry.EntryDate).Days;
                    var amount = Math.Abs(trans.DebitAmount - trans.CreditAmount);
                    
                    if (runningBalance > 0) // Customer owes us (debit balance)
                    {
                        if (daysOld <= 30)
                            agingItem.Current += amount;
                        else if (daysOld <= 60)
                            agingItem.Days30 += amount;
                        else if (daysOld <= 90)
                            agingItem.Days60 += amount;
                        else
                            agingItem.Days90 += amount;
                    }
                }

                agingItem.TotalOutstanding = agingItem.Current + agingItem.Days30 + agingItem.Days60 + agingItem.Days90;
                
                if (agingItem.TotalOutstanding > 0)
                {
                    report.Items.Add(agingItem);
                    report.TotalCurrent += agingItem.Current;
                    report.Total30Days += agingItem.Days30;
                    report.Total60Days += agingItem.Days60;
                    report.Total90Days += agingItem.Days90;
                }
            }

            report.GrandTotal = report.TotalCurrent + report.Total30Days + report.Total60Days + report.Total90Days;
            return report;
        }

        public async Task<ProfitLossReport> GenerateProfitLossReportAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new ProfitLossReport { FromDate = fromDate, ToDate = toDate };

            // Get Income groups (Revenue)
            var incomeGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Income" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in incomeGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, toDate);
                    if (balance != 0)
                    {
                        report.Income.Add(new ProfitLossItem
                        {
                            GroupName = group.GroupName,
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalIncome += Math.Abs(balance);
                    }
                }
            }

            // Get Expense groups
            var expenseGroups = await _context.AccountGroups
                .Where(g => g.GroupType == "Expense" && g.IsActive)
                .Include(g => g.Ledgers.Where(l => l.IsActive))
                .ToListAsync();

            foreach (var group in expenseGroups)
            {
                foreach (var ledger in group.Ledgers)
                {
                    var balance = await _ledgerService.GetLedgerBalanceAsync(ledger.LedgerID, toDate);
                    if (balance != 0)
                    {
                        report.Expenses.Add(new ProfitLossItem
                        {
                            GroupName = group.GroupName,
                            LedgerName = ledger.LedgerName,
                            Amount = Math.Abs(balance)
                        });
                        report.TotalExpenses += Math.Abs(balance);
                    }
                }
            }

            report.GrossProfit = report.TotalIncome;
            report.NetProfit = report.TotalIncome - report.TotalExpenses;

            return report;
        }
    }
}

