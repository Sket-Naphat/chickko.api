using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost()]
        public async Task<List<StockDto>> GetStockCostList(CostDto costDto)
        {
            try
            {
                //หาก่อนว่าในวันนี้มีรายการที่ต้องสั่งซื้อมั้ย
                var _cost = await _context.Cost.Where(c => c.CostDate == costDto.CostDate && !c.IsPurchese).ToListAsync();
                var StockDto = new List<StockDto>();
                if (_cost != null)
                {
                    //ถ้ามีที่ยังไม่จ่ายเงินทีให้ทำการ get stock และราคา
                    var _stock = await _context.Stock
                                .Include(s => s.StockCategory)
                                .Include(s => s.StockUnitType)
                                .Include(s => s.StockLocation)
                                .ToListAsync();

                    foreach (var stock in _stock)
                    {
                        var _stockDto = new StockDto
                        {
                            StockId = stock.StockId,
                            ItemName = stock.ItemName,
                            StockCategoryID = stock.StockCategoryID,
                            StockCategoryName = stock.StockCategory?.StockCategoryName ?? "",
                            StockUnitTypeID = stock.StockUnitTypeID,
                            StockUnitTypeName = stock.StockUnitType?.StockUnitTypeName ?? "",
                            StockLocationID = stock.StockLocationID,
                            StockLocationName = stock.StockLocation?.StockLocationName ?? "",
                            TotalQTY = stock.TotalQTY,
                            RequiredQTY = stock.RequiredQTY,
                            StockInQTY = stock.StockInQTY,
                            Remark = stock.Remark
                        };
                        StockDto.Add(_stockDto);
                    }
                }
                return StockDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกเมนูจาก Firestore");
                throw;
            }
        }
        public async Task CreateCost(Cost _Cost)
        {
            try
            {
                _context.Cost.Add(_Cost);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}