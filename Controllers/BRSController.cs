using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class BRSController : Controller
    {
        private readonly IBRSService _brsService;
        private readonly ApplicationDbContext _context;

        public BRSController(IBRSService brsService, ApplicationDbContext context)
        {
            _brsService = brsService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bankLedgers = await _context.Ledgers
                .Where(l => l.IsActive && l.LedgerName.Contains("Bank"))
                .ToListAsync();
            ViewBag.BankLedgers = bankLedgers;
            ViewBag.StatementDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? ledgerId, DateTime? statementDate)
        {
            var bankLedgers = await _context.Ledgers
                .Where(l => l.IsActive && l.LedgerName.Contains("Bank"))
                .ToListAsync();
            ViewBag.BankLedgers = bankLedgers;
            ViewBag.StatementDate = statementDate ?? DateTime.Now;

            if (ledgerId.HasValue)
            {
                var date = statementDate ?? DateTime.Now;
                var report = await _brsService.GenerateBRSReportAsync(ledgerId.Value, date);
                ViewBag.Report = report;
            }

            return View();
        }

        public async Task<IActionResult> Create(int ledgerId, DateTime statementDate)
        {
            var ledger = await _context.Ledgers.FindAsync(ledgerId);
            if (ledger == null)
                return NotFound();

            ViewBag.Ledger = ledger;
            ViewBag.StatementDate = statementDate;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Models.BankReconciliation reconciliation)
        {
            var result = await _brsService.CreateReconciliationAsync(reconciliation);
            if (result)
            {
                TempData["Success"] = "Bank Reconciliation created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(reconciliation);
        }

        public async Task<IActionResult> ExportPDF(int ledgerId, DateTime? statementDate)
        {
            var date = statementDate ?? DateTime.Now;
            var report = await _brsService.GenerateBRSReportAsync(ledgerId, date);
            // TODO: Implement PDF export
            return RedirectToAction(nameof(Index));
        }
    }
}

