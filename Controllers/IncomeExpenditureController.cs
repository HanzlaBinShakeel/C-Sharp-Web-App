using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class IncomeExpenditureController : Controller
    {
        private readonly IReportService _reportService;

        public IncomeExpenditureController(IReportService reportService)
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
            var report = await _reportService.GenerateIncomeExpenditureReportAsync(from, to);
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            return View(report);
        }

        public async Task<IActionResult> ExportPDF(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateIncomeExpenditureReportAsync(from, to);
            var pdf = _reportService.GeneratePDFReport(report, "IncomeExpenditure");
            return File(pdf, "application/pdf", $"IncomeExpenditure_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;
            var report = await _reportService.GenerateIncomeExpenditureReportAsync(from, to);
            var excel = _reportService.GenerateExcelReport(report, "IncomeExpenditure");
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"IncomeExpenditure_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }
    }
}

