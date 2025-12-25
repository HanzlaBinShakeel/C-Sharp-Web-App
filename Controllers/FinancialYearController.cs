using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class FinancialYearController : Controller
    {
        private readonly IFinancialYearService _financialYearService;

        public FinancialYearController(IFinancialYearService financialYearService)
        {
            _financialYearService = financialYearService;
        }

        public async Task<IActionResult> Index()
        {
            var years = await _financialYearService.GetAllFinancialYearsAsync();
            return View(years);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinancialYear financialYear)
        {
            if (ModelState.IsValid)
            {
                financialYear.CreatedBy = User.Identity?.Name;
                var result = await _financialYearService.CreateFinancialYearAsync(financialYear);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error creating financial year.");
            }
            return View(financialYear);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var year = await _financialYearService.GetFinancialYearByIdAsync(id);
            if (year == null)
            {
                return NotFound();
            }
            return View(year);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FinancialYear financialYear)
        {
            if (id != financialYear.FinancialYearID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _financialYearService.UpdateFinancialYearAsync(financialYear);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating financial year.");
            }
            return View(financialYear);
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(int id)
        {
            var result = await _financialYearService.SetActiveFinancialYearAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _financialYearService.CloseFinancialYearAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

