using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Models.ViewModels;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly ApplicationDbContext _context;

        public PaymentController(IVoucherService voucherService, ApplicationDbContext context)
        {
            _voucherService = voucherService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _context.JournalEntries
                .Where(e => e.VoucherType == "Payment" && e.IsPosted)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
            return View(payments);
        }

        public async Task<IActionResult> Create()
        {
            var ledgers = await _context.Ledgers
                .Where(l => l.IsActive)
                .Select(l => new { l.LedgerID, l.LedgerName, l.LedgerCode })
                .ToListAsync();
            ViewBag.Ledgers = ledgers;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentVoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                var result = await _voucherService.PostPaymentVoucherAsync(model, userId);
                if (result)
                {
                    TempData["Success"] = "Payment voucher posted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error posting payment voucher.");
            }

            var ledgers = await _context.Ledgers
                .Where(l => l.IsActive)
                .Select(l => new { l.LedgerID, l.LedgerName, l.LedgerCode })
                .ToListAsync();
            ViewBag.Ledgers = ledgers;
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var entry = await _context.JournalEntries
                .Include(e => e.JournalEntryLines)
                .ThenInclude(l => l.Ledger)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null)
                return NotFound();

            return View(entry);
        }
    }
}

