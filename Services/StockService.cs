using chickko.api.Data;
using chickko.api.Interface;

namespace chickko.api.Services
{
    public class StockService : IStockService
    {
        private readonly ChickkoContext _context;
         private readonly ILogger<StockService> _logger;
        public StockService(ChickkoContext context, ILogger<StockService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<string> GetStock()
        {
            throw new NotImplementedException();
        }
    } 
}