using chickko.api.Dtos;
using chickko.api.Models;
namespace chickko.api.Interface
{
    public interface ICostService
    {
        Task<List<StockDto>> GetStockCostList(CostDto costDto);
        Task<List<WorktimeDto>> GetWageCostList();
        Task CreateCost(Cost cost);
        Task UpdateStockCost(UpdateStockCostDto updateStockCostDto);
        Task<Cost> CreateCostReturnCostID(Cost cost);
        Task UpdateWageCost(WorktimeDto worktimeDto);
    }
}