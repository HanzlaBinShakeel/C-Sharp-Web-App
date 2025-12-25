using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class LedgerController : Controller
    {
        private readonly ILedgerService _ledgerService;
        private readonly IAccountGroupService _groupService;

        public LedgerController(ILedgerService ledgerService, IAccountGroupService groupService)
        {
            _ledgerService = ledgerService;
            _groupService = groupService;
        }

        public async Task<IActionResult> Index()
        {
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            return View(ledgers);
        }

        public async Task<IActionResult> Create()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            ViewBag.Groups = groups;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ledger ledger)
        {
            if (ModelState.IsValid)
            {
                var result = await _ledgerService.CreateLedgerAsync(ledger);
                if (result)
                {
                    TempData["Success"] = "Ledger created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Ledger code already exists or error occurred.");
            }

            var groups = await _groupService.GetAllGroupsAsync();
            ViewBag.Groups = groups;
            return View(ledger);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ledger = await _ledgerService.GetLedgerByIdAsync(id);
            if (ledger == null)
                return NotFound();

            var groups = await _groupService.GetAllGroupsAsync();
            ViewBag.Groups = groups;
            return View(ledger);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ledger ledger)
        {
            if (id != ledger.LedgerID)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _ledgerService.UpdateLedgerAsync(ledger);
                if (result)
                {
                    TempData["Success"] = "Ledger updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating ledger.");
            }

            var groups = await _groupService.GetAllGroupsAsync();
            ViewBag.Groups = groups;
            return View(ledger);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ledgerService.DeleteLedgerAsync(id);
            if (result)
            {
                TempData["Success"] = "Ledger deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Cannot delete ledger. It may have transactions.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

