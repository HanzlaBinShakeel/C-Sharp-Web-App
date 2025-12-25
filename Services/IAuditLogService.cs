using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string entityType, int entityId, string action, string? description = null, string? userId = null, string? userName = null, string? oldValues = null, string? newValues = null);
        Task<List<AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId);
        Task<List<AuditLog>> GetUserActivityAsync(string userName, int count = 10);
    }
}

