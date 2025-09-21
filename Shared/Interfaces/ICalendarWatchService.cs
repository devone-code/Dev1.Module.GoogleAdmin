using System;
using System.Threading.Tasks;
using Dev1.Module.GoogleAdmin.Shared.Models;

namespace Dev1.Module.GoogleAdmin.Services
{
    // Shared interface for server-side service implementation
    public interface ICalendarWatchService
    {
        Task<CalendarWatch> EnsureWatchAsync(int siteId, string calendarId, string userEmail, string webhookUrl);
        Task DecrementAsync(int siteId, string calendarId, string userEmail);
        Task<int> RenewExpiringAsync(int siteId, DateTime renewBeforeUtc);
        Task<int> CleanupAsync(int siteId, DateTime nowUtc);
        Task<CalendarWatch> GetByTokenAsync(string tokenKey);
    }
}
