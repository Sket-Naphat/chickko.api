using chickko.api.Dtos;
using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IStockService
    {
        Task<List<StockDto>> GetCurrentStock();
        Task<StockLog> CreateStockCountLog(StockCountDto stockCountDto, int costId);
        Task UpdateStockCountLog(List<StockCountDto> stockCountDto);
        Task CreateStockIn(StockInDto stockInDto);
        Task UpdateStockDetail(StockDto stockDto);
        Task<GetStockCountLogByCostId> GetStockCountLogByCostId(StockInDto stockCountDto);

    }
}