using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IAccountGroupService
    {
        Task<List<AccountGroup>> GetAllGroupsAsync();
        Task<AccountGroup?> GetGroupByIdAsync(int id);
        Task<AccountGroup?> GetGroupByCodeAsync(string code);
        Task<bool> CreateGroupAsync(AccountGroup group);
        Task<bool> UpdateGroupAsync(AccountGroup group);
        Task<bool> DeleteGroupAsync(int id);
        Task<List<AccountGroup>> GetGroupsByTypeAsync(string groupType);
    }
}

