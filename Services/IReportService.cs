using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IReportService
    {
        Task<BalanceSheetReport> GenerateBalanceSheetAsync(DateTime asOfDate);
        Task<TrialBalanceReport> GenerateTrialBalanceAsync(DateTime asOfDate);
        Task<ScheduleReport> GenerateScheduleReportAsync(int scheduleId, DateTime asOfDate);
        Task<CashbookReport> GenerateDoubleColumnCashbookAsync(DateTime fromDate, DateTime toDate);
        Task<ReceiptPaymentReport> GenerateReceiptPaymentReportAsync(DateTime fromDate, DateTime toDate);
        Task<IncomeExpenditureReport> GenerateIncomeExpenditureReportAsync(DateTime fromDate, DateTime toDate);
        Task<LedgerReconciliationReport> GenerateLedgerReconciliationAsync(int ledgerId, DateTime fromDate, DateTime toDate);
        Task<APAgingReport> GenerateAPAgingReportAsync(DateTime asOfDate);
        Task<ARAgingReport> GenerateARAgingReportAsync(DateTime asOfDate);
        Task<ProfitLossReport> GenerateProfitLossReportAsync(DateTime fromDate, DateTime toDate);
        byte[] GeneratePDFReport(object reportData, string reportType);
        byte[] GenerateExcelReport(object reportData, string reportType);
    }

    public class BalanceSheetReport
    {
        public DateTime AsOfDate { get; set; }
        public List<BalanceSheetItem> Assets { get; set; } = new();
        public List<BalanceSheetItem> Liabilities { get; set; } = new();
        public List<BalanceSheetItem> Equity { get; set; } = new();
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
    }

    public class BalanceSheetItem
    {
        public string GroupName { get; set; } = string.Empty;
        public string LedgerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TrialBalanceReport
    {
        public DateTime AsOfDate { get; set; }
        public List<TrialBalanceItem> Items { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }

    public class TrialBalanceItem
    {
        public string LedgerCode { get; set; } = string.Empty;
        public string LedgerName { get; set; } = string.Empty;
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
    }

    public class ScheduleReport
    {
        public int ScheduleID { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public DateTime AsOfDate { get; set; }
        public List<ScheduleItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class CashbookReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal OpeningCashBalance { get; set; }
        public decimal OpeningBankBalance { get; set; }
        public List<CashbookTransaction> Transactions { get; set; } = new();
        public decimal ClosingCashBalance { get; set; }
        public decimal ClosingBankBalance { get; set; }
    }

    public class CashbookTransaction
    {
        public DateTime Date { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string Particulars { get; set; } = string.Empty;
        public decimal CashReceipt { get; set; }
        public decimal CashPayment { get; set; }
        public decimal BankReceipt { get; set; }
        public decimal BankPayment { get; set; }
        public decimal CashBalance { get; set; }
        public decimal BankBalance { get; set; }
    }

    public class ReceiptPaymentReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ReceiptPaymentItem> Receipts { get; set; } = new();
        public List<ReceiptPaymentItem> Payments { get; set; } = new();
        public decimal TotalReceipts { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal NetCashFlow { get; set; }
    }

    public class ReceiptPaymentItem
    {
        public string LedgerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class IncomeExpenditureReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<IncomeExpenditureItem> Income { get; set; } = new();
        public List<IncomeExpenditureItem> Expenditure { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenditure { get; set; }
        public decimal SurplusDeficit { get; set; }
    }

    public class IncomeExpenditureItem
    {
        public string LedgerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class LedgerReconciliationReport
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<LedgerTransaction> Transactions { get; set; } = new();
        public decimal ClosingBalance { get; set; }
    }

    public class LedgerTransaction
    {
        public DateTime Date { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string Particulars { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    public class APAgingReport
    {
        public DateTime AsOfDate { get; set; }
        public List<APAgingItem> Items { get; set; } = new();
        public decimal TotalCurrent { get; set; }
        public decimal Total30Days { get; set; }
        public decimal Total60Days { get; set; }
        public decimal Total90Days { get; set; }
        public decimal TotalOver90Days { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class APAgingItem
    {
        public string VendorCode { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal Current { get; set; } // 0-30 days
        public decimal Days30 { get; set; } // 31-60 days
        public decimal Days60 { get; set; } // 61-90 days
        public decimal Days90 { get; set; } // 91+ days
        public decimal TotalOutstanding { get; set; }
    }

    public class ARAgingReport
    {
        public DateTime AsOfDate { get; set; }
        public List<ARAgingItem> Items { get; set; } = new();
        public decimal TotalCurrent { get; set; }
        public decimal Total30Days { get; set; }
        public decimal Total60Days { get; set; }
        public decimal Total90Days { get; set; }
        public decimal TotalOver90Days { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ARAgingItem
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Current { get; set; } // 0-30 days
        public decimal Days30 { get; set; } // 31-60 days
        public decimal Days60 { get; set; } // 61-90 days
        public decimal Days90 { get; set; } // 91+ days
        public decimal TotalOutstanding { get; set; }
    }

    public class ProfitLossReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ProfitLossItem> Income { get; set; } = new();
        public List<ProfitLossItem> Expenses { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class ProfitLossItem
    {
        public string GroupName { get; set; } = string.Empty;
        public string LedgerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}

