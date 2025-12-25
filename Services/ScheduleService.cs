using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule>> GetAllSchedulesAsync()
        {
            return await _context.Schedules
                .Include(s => s.LinkedLedger)
                .Include(s => s.ScheduleItems)
                .Where(s => s.IsActive)
                .OrderBy(s => s.ScheduleCode)
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleByIdAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.LinkedLedger)
                .Include(s => s.ScheduleItems)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
        }

        public async Task<bool> CreateScheduleAsync(Schedule schedule)
        {
            try
            {
                schedule.CreatedDate = DateTime.Now;
                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            try
            {
                var existing = await GetScheduleByIdAsync(schedule.ScheduleID);
                if (existing == null)
                    return false;

                existing.ScheduleCode = schedule.ScheduleCode;
                existing.ScheduleName = schedule.ScheduleName;
                existing.ScheduleType = schedule.ScheduleType;
                existing.Description = schedule.Description;
                existing.LinkedLedgerID = schedule.LinkedLedgerID;
                existing.IsActive = schedule.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            try
            {
                var schedule = await GetScheduleByIdAsync(id);
                if (schedule == null)
                    return false;

                schedule.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddScheduleItemAsync(int scheduleId, ScheduleItem item)
        {
            try
            {
                item.ScheduleID = scheduleId;
                item.CreatedDate = DateTime.Now;
                _context.ScheduleItems.Add(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateScheduleItemAsync(ScheduleItem item)
        {
            try
            {
                var existing = await _context.ScheduleItems.FindAsync(item.ItemID);
                if (existing == null)
                    return false;

                existing.ItemName = item.ItemName;
                existing.ItemCode = item.ItemCode;
                existing.Amount = item.Amount;
                existing.Remarks = item.Remarks;
                existing.ItemDate = item.ItemDate;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteScheduleItemAsync(int itemId)
        {
            try
            {
                var item = await _context.ScheduleItems.FindAsync(itemId);
                if (item == null)
                    return false;

                _context.ScheduleItems.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

