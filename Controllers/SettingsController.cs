using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Services;
using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IFinancialYearService _financialYearService;
        private readonly IVoucherNumberService _voucherNumberService;
        private readonly IReportService _reportService;

        public SettingsController(
            IFinancialYearService financialYearService,
            IVoucherNumberService voucherNumberService,
            IReportService reportService)
        {
            _financialYearService = financialYearService;
            _voucherNumberService = voucherNumberService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SettingsViewModel
            {
                FinancialYears = await _financialYearService.GetAllFinancialYearsAsync(),
                CurrentFinancialYear = await _financialYearService.GetActiveFinancialYearAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActiveFinancialYear(int financialYearId)
        {
            var result = await _financialYearService.SetActiveFinancialYearAsync(financialYearId);
            if (result)
            {
                TempData["SuccessMessage"] = "Active financial year updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update active financial year.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFinancialYear(FinancialYearViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var settingsModel = new SettingsViewModel
                {
                    FinancialYears = await _financialYearService.GetAllFinancialYearsAsync(),
                    CurrentFinancialYear = await _financialYearService.GetActiveFinancialYearAsync()
                };
                ViewBag.ShowCreateYearForm = true;
                return View("Index", settingsModel);
            }

            var financialYear = new FinancialYear
            {
                YearName = model.YearName,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            var result = await _financialYearService.CreateFinancialYearAsync(financialYear);
            if (result)
            {
                TempData["SuccessMessage"] = "Financial year created successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create financial year. Year name may already exist.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> VoucherSettings()
        {
            var activeYear = await _financialYearService.GetActiveFinancialYearAsync();
            if (activeYear == null)
            {
                TempData["ErrorMessage"] = "Please set an active financial year first.";
                return RedirectToAction(nameof(Index));
            }

            var voucherTypes = new[] { "Receipt", "Payment", "Journal", "Contra" };
            var voucherSettings = new List<VoucherSettingViewModel>();

            foreach (var type in voucherTypes)
            {
                var voucherNumber = await _voucherNumberService.GetVoucherNumberAsync(type, activeYear.FinancialYearID);
                voucherSettings.Add(new VoucherSettingViewModel
                {
                    VoucherType = type,
                    CurrentNumber = voucherNumber?.CurrentNumber ?? 1,
                    Prefix = voucherNumber?.Prefix ?? type.Substring(0, 1).ToUpper(),
                    FinancialYearID = activeYear.FinancialYearID
                });
            }

            ViewBag.ActiveYear = activeYear;
            return View(voucherSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVoucherSetting(VoucherSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(VoucherSettings));
            }

            var result = await _voucherNumberService.UpdateVoucherNumberAsync(
                model.VoucherType,
                model.FinancialYearID,
                model.Prefix,
                model.CurrentNumber,
                null);

            if (result)
            {
                TempData["SuccessMessage"] = $"{model.VoucherType} voucher settings updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to update {model.VoucherType} voucher settings.";
            }

            return RedirectToAction(nameof(VoucherSettings));
        }

        [HttpGet]
        public IActionResult SystemSettings()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReportSettings()
        {
            var model = new ReportSettingsViewModel
            {
                CompanyName = "Finance App", // Could be loaded from configuration
                CompanyAddress = "",
                ReportFooter = "Generated by Finance App"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReportSettings(ReportSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would save these to database or configuration
                TempData["SuccessMessage"] = "Report settings updated successfully!";
                return RedirectToAction(nameof(ReportSettings));
            }

            return View(model);
        }
    }

    public class SettingsViewModel
    {
        public List<FinancialYear> FinancialYears { get; set; } = new();
        public FinancialYear? CurrentFinancialYear { get; set; }
    }

    public class FinancialYearViewModel
    {
        [Required]
        [Display(Name = "Year Name")]
        public string YearName { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddYears(1);

        [Display(Name = "Set as Active")]
        public bool IsActive { get; set; }
    }

    public class VoucherSettingViewModel
    {
        public string VoucherType { get; set; } = "";
        public int CurrentNumber { get; set; } = 1;
        public string Prefix { get; set; } = "";
        public int FinancialYearID { get; set; }
    }

    public class ReportSettingsViewModel
    {
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = "";

        [Display(Name = "Company Address")]
        [DataType(DataType.MultilineText)]
        public string CompanyAddress { get; set; } = "";

        [Display(Name = "Report Footer")]
        [DataType(DataType.MultilineText)]
        public string ReportFooter { get; set; } = "";
    }
}

