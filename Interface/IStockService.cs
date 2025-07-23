using chickko.api.Dtos;

namespace chickko.api.Interface
{
    public interface IStockService
    {
        Task<List<StockDto>> GetCurrentStock();
        Task CreateStockCountLog(StockCountDto stockCountDto);
        Task CreateStocInLog(StockInDto stockInDto);
    }
}