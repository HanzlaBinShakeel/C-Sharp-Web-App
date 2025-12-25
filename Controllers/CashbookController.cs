using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class CashbookController : Controller
    {
        private readonly IReportService _reportService;

        public CashbookController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            ViewBag.FromDate = DateTime.Now.AddMonths(-1);
            ViewBag.ToDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateDoubleColumnCashbookAsync(from, to);
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            return View(report);
        }

        public async Task<IActionResult> ExportPDF(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateDoubleColumnCashbookAsync(from, to);
            var pdf = _reportService.GeneratePDFReport(report, "Cashbook");
            return File(pdf, "application/pdf", $"Cashbook_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateDoubleColumnCashbookAsync(from, to);
            var excel = _reportService.GenerateExcelReport(report, "Cashbook");
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Cashbook_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }
    }
}

