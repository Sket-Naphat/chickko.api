using chickko.api.Dtos.Event;
using chickko.api.Services.Event;

namespace chickko.api.Interface
{
    public interface IEventRollingService
    {
        Task<List<RollingRewardDto>> GetRollingRewardList();
    }
}