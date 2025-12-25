using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class ProfitLossController : Controller
    {
        private readonly IReportService _reportService;

        public ProfitLossController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            ViewBag.FromDate = new DateTime(DateTime.Now.Year, 4, 1); // Financial year start
            ViewBag.ToDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime fromDate, DateTime toDate)
        {
            var report = await _reportService.GenerateProfitLossReportAsync(fromDate, toDate);
            return View(report);
        }

        public async Task<IActionResult> ExportPDF(DateTime fromDate, DateTime toDate)
        {
            var report = await _reportService.GenerateProfitLossReportAsync(fromDate, toDate);
            var pdfBytes = _reportService.GeneratePDFReport(report, "ProfitLoss");
            return File(pdfBytes, "application/pdf", $"Profit_Loss_Report_{fromDate:yyyyMMdd}_to_{toDate:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(DateTime fromDate, DateTime toDate)
        {
            var report = await _reportService.GenerateProfitLossReportAsync(fromDate, toDate);
            var excelBytes = _reportService.GenerateExcelReport(report, "ProfitLoss");
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Profit_Loss_Report_{fromDate:yyyyMMdd}_to_{toDate:yyyyMMdd}.xlsx");
        }
    }
}

