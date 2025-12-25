using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ILedgerService _ledgerService;

        public CustomerController(ICustomerService customerService, ILedgerService ledgerService)
        {
            _customerService = customerService;
            _ledgerService = ledgerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Balance = await _customerService.GetCustomerBalanceAsync(id);
            ViewBag.Outstanding = await _customerService.GetCustomerOutstandingAsync(id);
            return View(customer);
        }

        public async Task<IActionResult> Create()
        {
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedBy = User.Identity?.Name;
                var result = await _customerService.CreateCustomerAsync(customer);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error creating customer. Customer code may already exist.");
            }
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View(customer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _customerService.UpdateCustomerAsync(customer);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating customer.");
            }
            var ledgers = await _ledgerService.GetAllLedgersAsync();
            ViewBag.Ledgers = new SelectList(ledgers, "LedgerID", "LedgerName");
            return View(customer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}

