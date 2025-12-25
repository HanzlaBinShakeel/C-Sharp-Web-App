using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class APAgingController : Controller
    {
        private readonly IReportService _reportService;

        public APAgingController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            ViewBag.AsOfDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime asOfDate)
        {
            var report = await _reportService.GenerateAPAgingReportAsync(asOfDate);
            return View(report);
        }

        public async Task<IActionResult> ExportPDF(DateTime asOfDate)
        {
            var report = await _reportService.GenerateAPAgingReportAsync(asOfDate);
            var pdfBytes = _reportService.GeneratePDFReport(report, "APAging");
            return File(pdfBytes, "application/pdf", $"AP_Aging_Report_{asOfDate:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(DateTime asOfDate)
        {
            var report = await _reportService.GenerateAPAgingReportAsync(asOfDate);
            var excelBytes = _reportService.GenerateExcelReport(report, "APAging");
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AP_Aging_Report_{asOfDate:yyyyMMdd}.xlsx");
        }
    }
}

