using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;

        public HomeController(ApplicationDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            // Get summary statistics for dashboard
            var totalIncome = await GetGroupTotalAsync("Income", DateTime.Now);
            var totalExpenses = await GetGroupTotalAsync("Expense", DateTime.Now);
            var cashBalance = await GetCashBalanceAsync();
            var netProfit = totalIncome - totalExpenses;

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.CashBalance = cashBalance;
            ViewBag.NetProfit = netProfit;

            return View();
        }

        private async Task<decimal> GetGroupTotalAsync(string groupType, DateTime asOfDate)
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
                    var balance = await _context.LedgerBalances
                        .Where(b => b.LedgerID == ledger.LedgerID && b.BalanceDate <= asOfDate)
                        .OrderByDescending(b => b.BalanceDate)
                        .Select(b => b.NetBalance)
                        .FirstOrDefaultAsync();
                    total += balance;
                }
            }

            return Math.Abs(total);
        }

        private async Task<decimal> GetCashBalanceAsync()
        {
            var cashLedger = await _context.Ledgers
                .FirstOrDefaultAsync(l => l.LedgerName.Contains("Cash") && l.IsActive);

            if (cashLedger == null)
                return 0;

            var balance = await _context.LedgerBalances
                .Where(b => b.LedgerID == cashLedger.LedgerID)
                .OrderByDescending(b => b.BalanceDate)
                .Select(b => b.NetBalance)
                .FirstOrDefaultAsync();

            return balance;
        }
    }
}

