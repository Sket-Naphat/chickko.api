namespace chickko.api.Dtos.Event
{
    public class RollingRewardDto
    {
        public int Id { get; set; }
        public string? RewardName { get; set; } = "";
        public string? Description { get; set; } = "";
        public decimal Probability { get; set; }
    }
}