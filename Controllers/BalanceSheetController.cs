using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class BalanceSheetController : Controller
    {
        private readonly IReportService _reportService;

        public BalanceSheetController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            ViewBag.AsOfDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateBalanceSheetAsync(date);
            ViewBag.AsOfDate = date;
            return View(report);
        }

        public async Task<IActionResult> ExportPDF(DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateBalanceSheetAsync(date);
            var pdf = _reportService.GeneratePDFReport(report, "BalanceSheet");
            return File(pdf, "application/pdf", $"BalanceSheet_{date:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateBalanceSheetAsync(date);
            var excel = _reportService.GenerateExcelReport(report, "BalanceSheet");
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BalanceSheet_{date:yyyyMMdd}.xlsx");
        }
    }
}

