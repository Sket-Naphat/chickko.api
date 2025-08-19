using chickko.api.Dtos;
using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IStockService
    {
        Task<List<StockDto>> GetCurrentStock();
        Task<StockLog> CreateStockCountLog(StockCountDto stockCountDto, int costId);
        Task UpdateStockCountLog(List<StockCountDto> stockCountDto);
        Task CreateStocInLog(StockInDto stockInDto);
        Task UpdateStockDetail(StockDto stockDto);
        Task<List<StockCountDto>> GetStockCountLogByCostId(StockInDto stockCountDto);

    }
}