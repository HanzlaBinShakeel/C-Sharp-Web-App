using FinanceApp.Models;

namespace FinanceApp.Services
{
    public interface IScheduleService
    {
        Task<List<Schedule>> GetAllSchedulesAsync();
        Task<Schedule?> GetScheduleByIdAsync(int id);
        Task<bool> CreateScheduleAsync(Schedule schedule);
        Task<bool> UpdateScheduleAsync(Schedule schedule);
        Task<bool> DeleteScheduleAsync(int id);
        Task<bool> AddScheduleItemAsync(int scheduleId, ScheduleItem item);
        Task<bool> UpdateScheduleItemAsync(ScheduleItem item);
        Task<bool> DeleteScheduleItemAsync(int itemId);
    }
}

