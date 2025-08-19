using chickko.api.Dtos;
using chickko.api.Models;
namespace chickko.api.Interface
{
    public interface ICostService
    {
        Task<List<CostDto>> GetStockCostRequest(CostDto costDto);
        Task<List<StockDto>> GetStockCostList(CostDto costDto);
        Task<List<WorktimeDto>> GetWageCostList();
        Task<List<CostDto>> GetAllCostList(CostDto costDto);
        Task CreateCost(Cost cost);
        Task UpdateStockCost(UpdateStockCostDto updateStockCostDto);
        Task<Cost> CreateCostReturnCostID(Cost cost);
        Task UpdateWageCost(WorktimeDto worktimeDto);
        Task<List<CostCategory>> GetCostCategoryList();
        Task UpdatePurchaseCost(Cost cost);
        Task UpdateStockCostDate(DateOnly costDate, int costId, int UpdateBy);
    }
}