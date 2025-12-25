using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class LedgerReconciliationController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;

        public LedgerReconciliationController(IReportService reportService, ApplicationDbContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ledgers = await _context.Ledgers.Where(l => l.IsActive).ToListAsync();
            ViewBag.Ledgers = ledgers;
            ViewBag.FromDate = DateTime.Now.AddMonths(-1);
            ViewBag.ToDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? ledgerId, DateTime? fromDate, DateTime? toDate)
        {
            var ledgers = await _context.Ledgers.Where(l => l.IsActive).ToListAsync();
            ViewBag.Ledgers = ledgers;
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;

            if (ledgerId.HasValue)
            {
                var report = await _reportService.GenerateLedgerReconciliationAsync(ledgerId.Value, from, to);
                ViewBag.Report = report;
            }

            return View();
        }

        public async Task<IActionResult> ExportPDF(int ledgerId, DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateLedgerReconciliationAsync(ledgerId, from, to);
            var pdf = _reportService.GeneratePDFReport(report, "LedgerReconciliation");
            return File(pdf, "application/pdf", $"LedgerReconciliation_{ledgerId}_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(int ledgerId, DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateLedgerReconciliationAsync(ledgerId, from, to);
            var excel = _reportService.GenerateExcelReport(report, "LedgerReconciliation");
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"LedgerReconciliation_{ledgerId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }
    }
}

