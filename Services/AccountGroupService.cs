using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class AccountGroupService : IAccountGroupService
    {
        private readonly ApplicationDbContext _context;

        public AccountGroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AccountGroup>> GetAllGroupsAsync()
        {
            return await _context.AccountGroups
                .Include(g => g.ParentGroup)
                .Where(g => g.IsActive)
                .OrderBy(g => g.GroupCode)
                .ToListAsync();
        }

        public async Task<AccountGroup?> GetGroupByIdAsync(int id)
        {
            return await _context.AccountGroups
                .Include(g => g.ParentGroup)
                .Include(g => g.SubGroups)
                .FirstOrDefaultAsync(g => g.GroupID == id);
        }

        public async Task<AccountGroup?> GetGroupByCodeAsync(string code)
        {
            return await _context.AccountGroups
                .FirstOrDefaultAsync(g => g.GroupCode == code);
        }

        public async Task<bool> CreateGroupAsync(AccountGroup group)
        {
            try
            {
                // Check if code already exists
                var existing = await GetGroupByCodeAsync(group.GroupCode);
                if (existing != null)
                    return false;

                group.CreatedDate = DateTime.Now;
                _context.AccountGroups.Add(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateGroupAsync(AccountGroup group)
        {
            try
            {
                var existing = await GetGroupByIdAsync(group.GroupID);
                if (existing == null)
                    return false;

                // Check if code is being changed and if new code already exists
                if (existing.GroupCode != group.GroupCode)
                {
                    var codeExists = await GetGroupByCodeAsync(group.GroupCode);
                    if (codeExists != null)
                        return false;
                }

                existing.GroupCode = group.GroupCode;
                existing.GroupName = group.GroupName;
                existing.GroupType = group.GroupType;
                existing.ParentGroupID = group.ParentGroupID;
                existing.IsActive = group.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            try
            {
                var group = await GetGroupByIdAsync(id);
                if (group == null)
                    return false;

                // Check if group has ledgers
                var hasLedgers = await _context.Ledgers.AnyAsync(l => l.GroupID == id && l.IsActive);
                if (hasLedgers)
                    return false; // Cannot delete group with active ledgers

                group.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<AccountGroup>> GetGroupsByTypeAsync(string groupType)
        {
            return await _context.AccountGroups
                .Where(g => g.GroupType == groupType && g.IsActive)
                .OrderBy(g => g.GroupCode)
                .ToListAsync();
        }
    }
}

