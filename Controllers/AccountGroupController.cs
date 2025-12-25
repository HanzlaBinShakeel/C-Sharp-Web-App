using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class AccountGroupController : Controller
    {
        private readonly IAccountGroupService _groupService;

        public AccountGroupController(IAccountGroupService groupService)
        {
            _groupService = groupService;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return View(groups);
        }

        public async Task<IActionResult> Create()
        {
            var parentGroups = await _groupService.GetAllGroupsAsync();
            ViewBag.ParentGroups = parentGroups;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountGroup group)
        {
            if (ModelState.IsValid)
            {
                var result = await _groupService.CreateGroupAsync(group);
                if (result)
                {
                    TempData["Success"] = "Group created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Group code already exists or error occurred.");
            }

            var parentGroups = await _groupService.GetAllGroupsAsync();
            ViewBag.ParentGroups = parentGroups;
            return View(group);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound();

            var parentGroups = await _groupService.GetAllGroupsAsync();
            ViewBag.ParentGroups = parentGroups;
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccountGroup group)
        {
            if (id != group.GroupID)
                return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _groupService.UpdateGroupAsync(group);
                if (result)
                {
                    TempData["Success"] = "Group updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating group.");
            }

            var parentGroups = await _groupService.GetAllGroupsAsync();
            ViewBag.ParentGroups = parentGroups;
            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _groupService.DeleteGroupAsync(id);
            if (result)
            {
                TempData["Success"] = "Group deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Cannot delete group. It may have active ledgers.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

