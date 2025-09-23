using System.ComponentModel.DataAnnotations;

namespace chickko.api.Dtos.Event
{
    public class EventRollingReward
    {
        [Key]
        public int RollingRewardId { get; set; }
        public string? RewardName { get; set; } = "";
        public string? Description { get; set; } = "";
        public decimal Probability { get; set; }
    }

    public class EventRollingResult
    {
        [Key]
        public int RollingResultId { get; set; }
        public string? OrderFirstStoreID { get; set; } = "";
        public string? CustomerName { get; set; } = "";
        public int RewardID { get; set; }
        public int CostPrice { get; set; }
        public EventRollingReward? Reward { get; set; }
        public DateOnly CreatedDate { get; set; }
        public TimeOnly CreatedTime { get; set; }
    }
}