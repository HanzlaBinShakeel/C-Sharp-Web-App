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
    public class JournalController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly ApplicationDbContext _context;

        public JournalController(IVoucherService voucherService, ApplicationDbContext context)
        {
            _voucherService = voucherService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var journals = await _context.JournalEntries
                .Where(e => e.VoucherType == "Journal" && e.IsPosted)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
            return View(journals);
        }

        public async Task<IActionResult> Create()
        {
            var ledgers = await _context.Ledgers
                .Where(l => l.IsActive)
                .Select(l => new { l.LedgerID, l.LedgerName, l.LedgerCode })
                .ToListAsync();
            ViewBag.Ledgers = ledgers;

            var model = new VoucherViewModel
            {
                EntryDate = DateTime.Now,
                VoucherType = "Journal",
                Lines = new List<VoucherLineViewModel> { new VoucherLineViewModel(), new VoucherLineViewModel() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
            // Validate double entry
            if (!await _voucherService.ValidateDoubleEntryAsync(model.Lines))
            {
                ModelState.AddModelError("", "Total Debits must equal Total Credits. Each line must have either Debit OR Credit, not both.");
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                var result = await _voucherService.PostJournalVoucherAsync(model, userId);
                if (result)
                {
                    TempData["Success"] = "Journal voucher posted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error posting journal voucher.");
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

