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
    public class ContraController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly ApplicationDbContext _context;

        public ContraController(IVoucherService voucherService, ApplicationDbContext context)
        {
            _voucherService = voucherService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contras = await _context.JournalEntries
                .Where(e => e.VoucherType == "Contra" && e.IsPosted)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
            return View(contras);
        }

        public async Task<IActionResult> Create()
        {
            // Get only Cash and Bank ledgers
            var cashBankLedgers = await _context.Ledgers
                .Where(l => l.IsActive && (l.LedgerName.Contains("Cash") || l.LedgerName.Contains("Bank")))
                .Select(l => new { l.LedgerID, l.LedgerName, l.LedgerCode })
                .ToListAsync();
            ViewBag.Ledgers = cashBankLedgers;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContraVoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                var result = await _voucherService.PostContraVoucherAsync(model, userId);
                if (result)
                {
                    TempData["Success"] = "Contra voucher posted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error posting contra voucher.");
            }

            var cashBankLedgers = await _context.Ledgers
                .Where(l => l.IsActive && (l.LedgerName.Contains("Cash") || l.LedgerName.Contains("Bank")))
                .Select(l => new { l.LedgerID, l.LedgerName, l.LedgerCode })
                .ToListAsync();
            ViewBag.Ledgers = cashBankLedgers;
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

