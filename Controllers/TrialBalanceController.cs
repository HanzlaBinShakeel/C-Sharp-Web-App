using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class TrialBalanceController : Controller
    {
        private readonly IReportService _reportService;

        public TrialBalanceController(IReportService reportService)
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
            var report = await _reportService.GenerateTrialBalanceAsync(date);
            ViewBag.AsOfDate = date;
            return View(report);
        }

        public async Task<IActionResult> ExportPDF(DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateTrialBalanceAsync(date);
            var pdf = _reportService.GeneratePDFReport(report, "TrialBalance");
            return File(pdf, "application/pdf", $"TrialBalance_{date:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateTrialBalanceAsync(date);
            var excel = _reportService.GenerateExcelReport(report, "TrialBalance");
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TrialBalance_{date:yyyyMMdd}.xlsx");
        }
    }
}

