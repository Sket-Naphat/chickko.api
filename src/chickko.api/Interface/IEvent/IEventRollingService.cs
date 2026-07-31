using chickko.api.Dtos.Event;
using chickko.api.Services.Event;

namespace chickko.api.Interface
{
    public interface IEventRollingService
    {
        Task<List<RollingRewardDto>> GetRollingRewardList();
        Task SaveRollingGameReward(RollingResultDto resultDto);
        Task<RollingResultDto> GetHistoryRollingGame(string OrderFirstStoreID);
        Task AddRollingReward(RollingRewardDto rewardDto);
        Task UpdateRollingReward(RollingRewardDto rewardDto);
        Task DeleteRollingReward(int rollingRewardId);
        Task<List<RollingResultDto>> GetRollingGameReport(DateOnly? date);
    }
}