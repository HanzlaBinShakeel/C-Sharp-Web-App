using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class ScheduleReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;

        public ScheduleReportController(IReportService reportService, ApplicationDbContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedules.Where(s => s.IsActive).ToListAsync();
            ViewBag.Schedules = schedules;
            ViewBag.AsOfDate = DateTime.Now;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? scheduleId, DateTime? asOfDate)
        {
            var schedules = await _context.Schedules.Where(s => s.IsActive).ToListAsync();
            ViewBag.Schedules = schedules;
            ViewBag.AsOfDate = asOfDate ?? DateTime.Now;

            if (scheduleId.HasValue)
            {
                var date = asOfDate ?? DateTime.Now;
                var report = await _reportService.GenerateScheduleReportAsync(scheduleId.Value, date);
                ViewBag.Report = report;
            }

            return View();
        }

        public async Task<IActionResult> ExportPDF(int scheduleId, DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateScheduleReportAsync(scheduleId, date);
            var pdf = _reportService.GeneratePDFReport(report, "Schedule");
            return File(pdf, "application/pdf", $"ScheduleReport_{scheduleId}_{date:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExportExcel(int scheduleId, DateTime? asOfDate)
        {
            var date = asOfDate ?? DateTime.Now;
            var report = await _reportService.GenerateScheduleReportAsync(scheduleId, date);
            var excel = _reportService.GenerateExcelReport(report, "Schedule");
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ScheduleReport_{scheduleId}_{date:yyyyMMdd}.xlsx");
        }
    }
}

