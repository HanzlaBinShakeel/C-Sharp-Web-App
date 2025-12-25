using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string entityType, int entityId, string action, string? description = null, string? userId = null, string? userName = null, string? oldValues = null, string? newValues = null)
        {
            try
            {
                var log = new AuditLog
                {
                    EntityType = entityType,
                    EntityID = entityId,
                    Action = action,
                    Description = description,
                    UserID = userId ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    UserName = userName ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IPAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    ActionDate = DateTime.Now,
                    OldValues = oldValues,
                    NewValues = newValues
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Silently fail - audit logging should not break the application
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            if (entityId.HasValue)
            {
                query = query.Where(a => a.EntityID == entityId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.ActionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.ActionDate <= toDate.Value);
            }

            return await query
                .OrderByDescending(a => a.ActionDate)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.ActionDate)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetUserActivityAsync(string userName, int count = 10)
        {
            return await _context.AuditLogs
                .Where(a => a.UserName == userName)
                .OrderByDescending(a => a.ActionDate)
                .Take(count)
                .ToListAsync();
        }
    }
}

