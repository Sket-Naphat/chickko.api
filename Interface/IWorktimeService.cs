using chickko.api.Dtos;

namespace chickko.api.Interface
{
    public interface IWorktimeService
    {
        Task<string> ClockIn(WorktimeDto WorktimeDto);
        Task<string> ClockOut(WorktimeDto WorktimeDto);
    }
}