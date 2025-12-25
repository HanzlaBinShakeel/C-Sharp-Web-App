using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Models;
using FinanceApp.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditLogService _auditLogService;

        public ProfileController(UserManager<IdentityUser> userManager, IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime
            };

            // Get recent activity
            ViewBag.RecentActivity = await _auditLogService.GetUserActivityAsync(user.UserName ?? "", 10);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.RecentActivity = await _auditLogService.GetUserActivityAsync(user.UserName ?? "", 10);
                }
                return View("Index", model);
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            // Update user properties
            if (userToUpdate.Email != model.Email)
            {
                userToUpdate.Email = model.Email;
                userToUpdate.UserName = model.Email; // Update username to match email
                userToUpdate.NormalizedUserName = model.Email.ToUpper();
                userToUpdate.NormalizedEmail = model.Email.ToUpper();
            }

            userToUpdate.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(userToUpdate);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.RecentActivity = await _auditLogService.GetUserActivityAsync(userToUpdate.UserName ?? "", 10);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var profileModel = new ProfileViewModel
                    {
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumber = user.PhoneNumber,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd?.DateTime
                    };
                    ViewBag.RecentActivity = await _auditLogService.GetUserActivityAsync(user.UserName ?? "", 10);
                    ViewBag.ShowPasswordForm = true;
                    return View("Index", profileModel);
                }
                return RedirectToAction(nameof(Index));
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(userToUpdate, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var profileModel2 = new ProfileViewModel
            {
                UserName = userToUpdate.UserName ?? "",
                Email = userToUpdate.Email ?? "",
                EmailConfirmed = userToUpdate.EmailConfirmed,
                PhoneNumber = userToUpdate.PhoneNumber,
                TwoFactorEnabled = userToUpdate.TwoFactorEnabled,
                LockoutEnabled = userToUpdate.LockoutEnabled,
                LockoutEnd = userToUpdate.LockoutEnd?.DateTime
            };
            ViewBag.RecentActivity = await _auditLogService.GetUserActivityAsync(userToUpdate.UserName ?? "", 10);
            ViewBag.ShowPasswordForm = true;
            return View("Index", profileModel2);
        }
    }

    public class ProfileViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}

