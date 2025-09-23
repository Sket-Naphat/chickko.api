using chickko.api.Data;
using chickko.api.Dtos.Event;
using chickko.api.Interface;
using Google.Api;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services.Event
{
    public class EventRollingService : IEventRollingService
    {
        private readonly ChickkoContext _context;
        public EventRollingService(ChickkoContext context)
        {
            _context = context;
        }

        public async Task<List<RollingRewardDto>> GetRollingRewardList()
        {
            var rewardList = await _context.EventRollingRewards.ToListAsync();
            var dtoList = rewardList.Select(r => new RollingRewardDto
            {
                RollingRewardId = r.RollingRewardId,
                RewardName = r.RewardName,
                Description = r.Description,
                Probability = r.Probability
                // Add other properties as needed
            }).ToList();
            return dtoList;
        }
        public async Task SaveRollingGameReward(RollingResultDto resultDto)
        {
            var existingResult = await _context.EventRollingResults
                .FirstOrDefaultAsync(r => r.OrderFirstStoreID == resultDto.OrderFirstStoreID);

            if (existingResult != null)
            {
                // Update existing result
                existingResult.CustomerName = resultDto.CustomerName;
                existingResult.RewardID = resultDto.RewardID;
                existingResult.OrderFirstStoreID = resultDto.OrderFirstStoreID;
                existingResult.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
                existingResult.CreatedTime = TimeOnly.FromDateTime(DateTime.Now);
                // Update other properties as needed
            }
            else
            {
                var result = new EventRollingResult
                {
                    CustomerName = resultDto.CustomerName,
                    OrderFirstStoreID = resultDto.OrderFirstStoreID,
                    RewardID = resultDto.RewardID,
                    CostPrice = resultDto.CostPrice,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    CreatedTime = TimeOnly.FromDateTime(DateTime.Now)
                    // Map other properties as needed
                };

                _context.EventRollingResults.Add(result);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<RollingResultDto> GetHistoryRollingGame(string OrderFirstStoreID)
        {
            try
            {
                var result = await _context.EventRollingResults
                  .Include(r => r.Reward)
                  .FirstOrDefaultAsync(r => r.OrderFirstStoreID == OrderFirstStoreID);

                if (result == null)
                {
                    // Return a default-initialized RollingResultDto to avoid null reference return
                    return new RollingResultDto();
                }

                var dto = new RollingResultDto
                {
                    RollingResultId = result.RollingResultId,
                    OrderFirstStoreID = result.OrderFirstStoreID,
                    RewardID = result.RewardID,
                    CostPrice = result.CostPrice,
                    CreatedDate = result.CreatedDate,
                    CreatedTime = result.CreatedTime,
                    Reward = result.Reward != null ? new RollingRewardDto
                    {
                        RollingRewardId = result.Reward.RollingRewardId,
                        RewardName = result.Reward.RewardName,
                        Description = result.Reward.Description,
                        Probability = result.Reward.Probability
                        // Map other properties as needed
                    } : null
                };

                return dto;
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
                throw new Exception("An error occurred while processing the request.", ex);
            }

        }
        public async Task AddRollingReward(RollingRewardDto rewardDto)
        {
            var reward = new EventRollingReward
            {
                RewardName = rewardDto.RewardName,
                Description = rewardDto.Description,
                Probability = rewardDto.Probability
                // Map other properties as needed
            };

            _context.EventRollingRewards.Add(reward);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateRollingReward(RollingRewardDto rewardDto)
        {
            try
            {
                var existingReward = await _context.EventRollingRewards
                .FirstOrDefaultAsync(r => r.RollingRewardId == rewardDto.RollingRewardId);

                if (existingReward != null)
                {
                    existingReward.RewardName = rewardDto.RewardName;
                    existingReward.Description = rewardDto.Description;
                    existingReward.Probability = rewardDto.Probability;
                    // Update other properties as needed

                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Reward not found");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("A concurrency error occurred while updating the reward.");
            }

        }
        public async Task DeleteRollingReward(int rollingRewardId)
        {
            var existingReward = await _context.EventRollingRewards
                .FirstOrDefaultAsync(r => r.RollingRewardId == rollingRewardId);

            if (existingReward != null)
            {
                _context.EventRollingRewards.Remove(existingReward);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Reward not found");
            }
        }
        public async Task<List<RollingResultDto>> GetRollingGameReport(DateOnly? date)
        {
            var query = _context.EventRollingResults
                .Include(r => r.Reward)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(r => r.CreatedDate == date.Value);
            }

            var results = await query.ToListAsync();

            var dtoList = results.Select(r => new RollingResultDto
            {
                RollingResultId = r.RollingResultId,
                OrderFirstStoreID = r.OrderFirstStoreID,
                RewardID = r.RewardID,
                CostPrice = r.CostPrice,
                CreatedDate = r.CreatedDate,
                CreatedTime = r.CreatedTime,
                CustomerName = r.CustomerName,
                Reward = r.Reward != null ? new RollingRewardDto
                {
                    RollingRewardId = r.Reward.RollingRewardId,
                    RewardName = r.Reward.RewardName,
                    Description = r.Reward.Description,
                    Probability = r.Reward.Probability
                    // Map other properties as needed
                } : null
            }).ToList();

            return dtoList;
        }
    }
}