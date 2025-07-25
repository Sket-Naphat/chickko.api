using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;

namespace chickko.api.Services
{
    public class CostService : ICostService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<StockService> _logger;
        public CostService(ChickkoContext context, ILogger<StockService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<string> addNewCost(CostDto costDto)
        {
            throw new NotImplementedException();
        }
    }
}