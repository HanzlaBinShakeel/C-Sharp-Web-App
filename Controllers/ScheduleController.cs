using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly ApplicationDbContext _context;

        public ScheduleController(IScheduleService scheduleService, ApplicationDbContext context)
        {
            _scheduleService = scheduleService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _scheduleService.GetAllSchedulesAsync();
            return View(schedules);
        }

        public async Task<IActionResult> Create()
        {
            var ledgers = await _context.Ledgers.Where(l => l.IsActive).ToListAsync();
            ViewBag.Ledgers = ledgers;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                var result = await _scheduleService.CreateScheduleAsync(schedule);
                if (result)
                {
                    TempData["Success"] = "Schedule created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error creating schedule.");
            }

            var ledgers = await _context.Ledgers.Where(l => l.IsActive).ToListAsync();
            ViewBag.Ledgers = ledgers;
            return View(schedule);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound();

            var ledgers = await _context.Ledgers.Where(l => l.IsActive).ToListAsync();
            ViewBag.Ledgers = ledgers;
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            if (id != schedule.ScheduleID)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _scheduleService.UpdateScheduleAsync(schedule);
                if (result)
                {
                    TempData["Success"] = "Schedule updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating schedule.");
            }

            var ledgers = await _context.Ledgers.Where(l => l.IsActive).ToListAsync();
            ViewBag.Ledgers = ledgers;
            return View(schedule);
        }

        public async Task<IActionResult> Details(int id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int scheduleId, ScheduleItem item)
        {
            var result = await _scheduleService.AddScheduleItemAsync(scheduleId, item);
            if (result)
            {
                TempData["Success"] = "Item added successfully!";
            }
            return RedirectToAction(nameof(Details), new { id = scheduleId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteItem(int itemId, int scheduleId)
        {
            var result = await _scheduleService.DeleteScheduleItemAsync(itemId);
            if (result)
            {
                TempData["Success"] = "Item deleted successfully!";
            }
            return RedirectToAction(nameof(Details), new { id = scheduleId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _scheduleService.DeleteScheduleAsync(id);
            if (result)
            {
                TempData["Success"] = "Schedule deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

