using chickko.api.Dtos;

namespace chickko.api.Interface
{
    public interface IWorktimeService
    {
        Task<string> ClockIn(WorktimeDto WorktimeDto);
        Task<string> ClockOut(WorktimeDto WorktimeDto);
        Task<WorktimeDto> GetPeriodWorktimeByEmployeeID(WorktimeDto WorktimeDto);
        Task<List<WorktimeDto>> GetWorkTimeHistoryByEmployeeID(WorktimeDto WorktimeDto);
        Task<List<WorktimeDto>> GetWorkTimeHistoryByPeriod(WorktimeDto WorktimeDto);
        Task<WorktimeSummaryDto> GetWorkTimeCostByEmployeeIDandPeriod(WorktimeDto worktimeDto);
        Task<string> UpdateTimeClockIn(WorktimeDto worktimeDto);
        Task<string> UpdateTimeClockOut(WorktimeDto worktimeDto);
    }
}