using chickko.api.Dtos;
using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IStockService
    {
        Task<List<StockDto>> GetCurrentStock();
        Task<List<StockDto>> GetAllStockItem();
        Task<StockLog> CreateStockCountLog(StockCountDto stockCountDto, int costId);
        Task UpdateStockCountLog(List<StockCountDto> stockCountDto ,int CostId);
        Task CreateStockIn(StockInDto stockInDto, int costId);
        Task CreateStockDetail(StockDto stockDto);
        Task UpdateStockDetail(StockDto stockDto);
        Task<GetStockCountLogByCostId> GetStockCountLogByCostId(StockInDto stockCountDto);
        Task<List<StockUnitType>> GetStockUnitType();
        Task<List<StockLocation>> GetStockLocation();
        Task<List<StockCategory>> GetStockCategory();
    }
}