using chickko.api.Dtos;
using chickko.api.Models;
namespace chickko.api.Interface
{
    public interface ICostService
    {
        Task<List<StockDto>> GetStockCostList(CostDto costDto);
        Task CreateCost(Cost cost);
        Task UpdateStockCost(UpdateStockCostDto updateStockCostDto);
        Task<Cost> CreateCostReturnCostID(Cost cost);
    }
}