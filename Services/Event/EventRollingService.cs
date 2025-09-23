using chickko.api.Dtos.Event;
using chickko.api.Interface;

namespace chickko.api.Services.Event
{
    public class EventRollingService : IEventRollingService
    {
        public EventRollingService()
        {
        }

        public async Task<List<RollingRewardDto>> GetRollingRewardList()
        {
            // Simulate async operation
            await Task.Delay(100);

            var rewardList = new List<RollingRewardDto>
            {
                new RollingRewardDto { Id = 1, RewardName = "รางวัลที่ 1", Description = "รางวัลพิเศษ", Probability = 0.1M },
                new RollingRewardDto { Id = 2, RewardName = "รางวัลที่ 2", Description = "รางวัลปลอบใจ", Probability = 0.9M },
                new RollingRewardDto { Id = 3, RewardName = "รางวัลที่ 3", Description = "รางวัลปลอบใจ", Probability = 0.9M }
            };

            return rewardList;
        }
    }
}