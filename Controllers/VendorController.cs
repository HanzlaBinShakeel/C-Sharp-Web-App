using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class VendorController : Controller
    {
        private readonly IVendorService _vendorService;
        private readonly ILedgerService _ledgerService;

        public VendorController(IVendorService vendorService, ILedgerService ledgerService)
        {
            _vendorService = vendorService;
            _ledgerService = ledgerService;
        }

        public async Task<IActionResult> Index()
        {
            var vendors = await _vendorService.GetAllVendorsAsync();
            return View(vendors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            ViewBag.Balance = await _vendorService.GetVendorBalanceAsync(id);
            ViewBag.Outstanding = await _vendorService.GetVendorOutstandingAsync(id);
            return View(vendor);
        }

        public async Task<IActionResult> Create()
        {
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                vendor.CreatedBy = User.Identity?.Name;
                var result = await _vendorService.CreateVendorAsync(vendor);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error creating vendor. Vendor code may already exist.");
            }
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View(vendor);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View(vendor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vendor vendor)
        {
            if (id != vendor.VendorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _vendorService.UpdateVendorAsync(vendor);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating vendor.");
            }
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View(vendor);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            return View(vendor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _vendorService.DeleteVendorAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}

