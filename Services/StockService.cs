using System.Globalization;
using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.EntityFrameworkCore;

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
        public async Task<List<StockDto>> GetCurrentStock()
        {
            try
            {
                var stocks = await _context.Stocks
                    .Include(s => s.StockCategory)
                    .Include(s => s.StockUnitType)
                    .Include(s => s.StockLocation)
                    .Where(s => s.Active == true)
                    .OrderBy(s => s.ItemName)
                    .ToListAsync();

                var stockDtos = stocks.Select(s => new StockDto
                {
                    StockId = s.StockId,
                    ItemName = s.ItemName,
                    StockCategoryID = s.StockCategoryID,
                    StockCategoryName = s.StockCategory!.StockCategoryName,
                    StockUnitTypeID = s.StockUnitTypeID,
                    StockUnitTypeName = s.StockUnitType!.StockUnitTypeName,
                    StockLocationID = s.StockLocationID,
                    StockLocationName = s.StockLocation!.StockLocationName,
                    TotalQTY = s.TotalQTY,
                    RequiredQTY = s.RequiredQTY,
                    StockInQTY = s.StockInQTY,
                    UpdateDate = s.UpdateDate.ToString("yyyy-MM-dd"),
                    UpdateTime = s.UpdateTime.ToString("HH:mm:ss"),
                    Remark = s.Remark
                }).ToList();

                return stockDtos;
            }
            catch (Exception ex)
            {
                // 🔴 Log หรือแจ้ง error ที่นี่ตามเหมาะสม
                Console.WriteLine($"[ERROR] GetCurrentStock: {ex.Message}");

                // หรือโยนต่อไปให้ controller จัดการ (ถ้าคุณใช้ error middleware อยู่)
                throw;
            }
        }

        public async Task<GetStockCountLogByCostId> GetStockCountLogByCostId(StockInDto stockCountDto)
        {
            try
            {
                var costId = stockCountDto.CostId;
               
                var stockLogsQuery = _context.StockLogs
                    .Include(sl => sl.Stock)
                    .ThenInclude(s => s!.StockCategory)
                    .Include(sl => sl.Stock)
                    .ThenInclude(s => s!.StockUnitType)
                    .Include(sl => sl.Stock)
                    .ThenInclude(s => s!.StockLocation)
                    .Include(sl => sl.Cost)
                    .Where(sl => sl.Stock != null && sl.Stock.Active == true && sl.CostId == costId);

                if (stockCountDto.IsStockIn == true) //แสดงเฉพาะที่มีการสั่งซื้อ
                {
                    stockLogsQuery = stockLogsQuery.Where(sl => sl.StockInQTY > 0);
                }

                var stockLogs = await stockLogsQuery
                    .OrderByDescending(sl => sl.StockLogId)
                    .ToListAsync();


                var stockDtos = new List<StockCountDto>();
                if (stockLogs == null || stockLogs.Count == 0)
                {
                    return new GetStockCountLogByCostId
                    {
                        CostPrice = 0,
                        StockInDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        StockCountDtos = new List<StockCountDto>()
                    };
                }

                var CostPrice = stockLogs.FirstOrDefault()?.Cost?.CostPrice ?? 0;
                var StockInDate = stockLogs.FirstOrDefault()?.StockInDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                foreach (var sl in stockLogs)
                {
                    var dto = new StockCountDto
                    {
                        StockId = sl.Stock!.StockId,
                        StockLogId = sl.StockLogId,
                        ItemName = sl.Stock.ItemName,
                        StockCategoryID = sl.Stock.StockCategoryID,
                        StockCategoryName = sl.Stock.StockCategory?.StockCategoryName,
                        StockUnitTypeID = sl.Stock.StockUnitTypeID,
                        StockUnitTypeName = sl.Stock.StockUnitType?.StockUnitTypeName,
                        StockLocationID = sl.Stock.StockLocationID,
                        StockLocationName = sl.Stock.StockLocation?.StockLocationName,
                        RequiredQTY = sl.RequiredQTY,
                        TotalQTY = sl.TotalQTY ?? 0,
                        StockInQTY = sl.StockInQTY ?? 0,
                        Remark = sl.Remark ?? string.Empty,
                        StockLogTypeID = sl.StockLogTypeID,
                        CostId = sl.CostId,
                        PurchaseQTY = sl.PurchaseQTY ?? 0,
                        StockCountDate = sl.StockCountDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                        StockCountTime = sl.StockCountTime?.ToString("HH:mm:ss") ?? string.Empty
                    };
                    stockDtos.Add(dto);
                }

                return new GetStockCountLogByCostId
                {
                    CostPrice = CostPrice,
                    StockInDate = StockInDate ?? DateTime.Now.ToString("yyyy-MM-dd"),
                    StockCountDtos = stockDtos
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetCurrentStock: {ex.Message}");
                throw;
            }
        }

        public async Task<StockLog> CreateStockCountLog(StockCountDto stockCountDto, int costId)
        {
            try
            {
                var stock = await _context.Stocks.FindAsync(stockCountDto.StockId)
                ?? throw new Exception($"ไม่พบ Stock ID: {stockCountDto.StockId}");

                // 2. แปลงวันที่และเวลาให้อยู่ในรูปแบบ DateOnly และ TimeOnly
                //var date = DateOnly.Parse(stockCountDto.StockInDate);
                var date = DateOnly.TryParseExact(stockCountDto.StockCountDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                var time = TimeOnly.Parse(stockCountDto.StockCountTime);
                int RequiredQTY = stock.RequiredQTY;//ตรวจสอบก่อนว่ามี
                if ((RequiredQTY != stockCountDto.RequiredQTY) && stockCountDto.RequiredQTY != null && stockCountDto.RequiredQTY != 0)
                {
                    RequiredQTY = stockCountDto.RequiredQTY.Value;
                    stock.StockInQTY = RequiredQTY;
                }
                int StockInQTY = RequiredQTY - stockCountDto.TotalQTY;
                StockInQTY = (StockInQTY < 0) ? 0 : StockInQTY;

                // ถ้า stockCountDto.StockInQTY มีค่า → ใช้ค่านั้นแทน
                if (stockCountDto.StockInQTY != null)
                {
                    StockInQTY = stockCountDto.StockInQTY.Value;
                }
                // 3. สร้าง StockLog ใหม่สำหรับบันทึกการ "ขานับยอด"
                var stockLog = new StockLog
                {
                    StockId = stock.StockId,
                    StockCountDate = date,
                    StockCountTime = time,
                    TotalQTY = stockCountDto.TotalQTY, //จำนวนคงเหลือ
                    RequiredQTY = RequiredQTY, //จนวนที่ต้องใช้
                    StockInQTY = StockInQTY, //จำนวนที่ต้องซื้อ
                    StockLogTypeID = 1, // 1 = Count (ขานับ)
                    Remark = stockCountDto.Remark,
                    CostId = costId,
                    SupplyID = stockCountDto.SupplyId,
                    CreateBy = stockCountDto.UpdateBy ?? 0, // ใช้ UpdateBy ถ้ามี
                    CreateDate = DateOnly.FromDateTime(DateTime.Now),
                    CreateTime = TimeOnly.FromDateTime(DateTime.Now),
                };

                // 4. บันทึก StockLog ลงฐานข้อมูล
                _context.StockLogs.Add(stockLog);
                await _context.SaveChangesAsync();

                // 5. อัปเดตข้อมูลใน Stock หลักให้ตรงกับยอดคงเหลือใหม่
                stock.TotalQTY = stockCountDto.TotalQTY;
                stock.StockInQTY = StockInQTY;
                stock.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                stock.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);
                stock.RecentStockLogId = stockLog.StockLogId;

                // 6. บันทึกการอัปเดต Stock
                await _context.SaveChangesAsync();
                return stockLog;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Inner: " + ex.InnerException?.Message);
                throw; // หรือจะ return BadRequest ก็ได้
            }
            // 1. ดึง Stock หลักจาก DB โดยใช้ StockId ที่ผู้ใช้ส่งมา

        }
        public async Task CreateStockIn(StockInDto stockInDto)
        {
            // 1. ดึง Stock หลักจาก DB โดยใช้ StockId ที่ผู้ใช้ส่งมา
            try
            {
                var stockLog = await _context.StockLogs.FirstOrDefaultAsync(log => log.StockLogId == stockInDto.StockLogId)
                    ?? throw new Exception($"ไม่พบ StockLog ID: {stockInDto.StockLogId}");

                // 2. แปลงวันที่และเวลาให้อยู่ในรูปแบบ DateOnly และ TimeOnly
                var date = DateOnly.TryParseExact(stockInDto.StockInDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                var time = TimeOnly.TryParse(stockInDto.StockInTime, out var parsedTime) ? parsedTime : TimeOnly.FromDateTime(DateTime.Now);
                int dipQTY = stockInDto.PurchaseQTY - stockInDto.StockInQTY;

                // 3. update StockLog
                if (stockInDto.Price > 0)
                {
                    stockLog.Price = stockInDto.Price;
                    await UpdateStockCostUnit(stockInDto.StockId, stockInDto.Price, stockInDto.UpdateBy ?? 0);
                }
                stockLog.StockInDate = date;
                stockLog.StockInTime = time;
                stockLog.PurchaseQTY = stockInDto.PurchaseQTY;
                stockLog.DipQTY = dipQTY;
                stockLog.IsPurchase = stockInDto.IsPurchase;
                stockLog.Remark = stockInDto.Remark;
                stockLog.SupplyID = stockInDto.SupplyId;
                stockLog.StockLogTypeID = 2;
                stockLog.UpdateBy = stockInDto.UpdateBy ?? 0;
                stockLog.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                stockLog.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);

                // ไม่ควร Add ใหม่ ให้ใช้ Update
                _context.StockLogs.Update(stockLog);
                await _context.SaveChangesAsync();

                // 5. อัปเดต Stock หลักให้เพิ่มจำนวนที่ซื้อเข้ามา
                var stock = await _context.Stocks.FindAsync(stockLog.StockId)
                    ?? throw new Exception($"ไม่พบ Stock ID: {stockLog.StockId}");

                stock.TotalQTY += stockInDto.PurchaseQTY;
                stock.StockInQTY = Math.Max(0, stock.RequiredQTY - stock.TotalQTY); // คำนวณ StockInQTY ใหม่ ถ้า TotalQTY น้อยกว่า RequiredQTY จะเป็น 0
                stock.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                stock.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);
                stock.RecentStockLogId = stockLog.StockLogId;

                _context.Stocks.Update(stock);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateStockIn");
                throw;
            }
        }


        public async Task UpdateStockDetail(StockDto stockDto)
        {
            try
            {
                var stock = await _context.Stocks.FindAsync(stockDto.StockId)
                ?? throw new Exception($"ไม่พบ Stock ID: {stockDto.StockId}");

                stock.ItemName = stockDto.ItemName;
                stock.StockLocationID = stockDto.StockLocationID;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Inner: " + ex.InnerException?.Message);
                throw; // หรือจะ return BadRequest ก็ได้
            }
        }
        public async Task UpdateStockCountLog(List<StockCountDto> stockCountDto)
        {
            try
            {
                foreach (var stock in stockCountDto)
                {
                    var date = DateOnly.TryParseExact(stock.StockCountDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                    var time = TimeOnly.Parse(stock.StockCountTime);

                    var stockLog = await _context.StockLogs.FirstOrDefaultAsync(sl => sl.StockLogId == stock.StockLogId)
                        ?? throw new Exception($"ไม่พบ StockLog ID: {stock.StockLogId}");


                    stockLog.StockId = stock.StockId;
                    stockLog.StockCountDate = date;
                    stockLog.StockCountTime = time;
                    stockLog.TotalQTY = stock.TotalQTY; //จำนวนคงเหลือ
                    stockLog.RequiredQTY = stock.RequiredQTY; //จนวนที่ต้องใช้
                    stockLog.StockInQTY = stock.StockInQTY; //จำนวนที่ต้องซื้อ
                    stockLog.StockLogTypeID = 1; // 1 = Count (ขานับ)
                    stockLog.Remark = stock.Remark;
                    stockLog.SupplyID = stock.SupplyId;
                    stockLog.UpdateBy = stock.UpdateBy ?? 0; // ใช้ UpdateBy ถ้ามี
                    stockLog.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    stockLog.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Inner: " + ex.InnerException?.Message);
                throw; // หรือจะ return BadRequest ก็ได้
            }

        }
        public async Task UpdateStockCostUnit(int StockId, int StockUnitPrice, int UpdateBy)
        {
            try
            {
                var StockUnitCostHistory = await _context.StockUnitCostHistory
                    .Where(s => s.StockId == StockId)
                    .OrderByDescending(s => s.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (StockUnitCostHistory == null)
                {
                    StockUnitCostHistory = new StockUnitCostHistory
                    {
                        StockId = StockId,
                        CostPrice = StockUnitPrice,
                        EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
                        CreatedBy = UpdateBy, // หรือกำหนดค่าอื่นตามที่ต้องการ
                        CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                        CreatedTime = TimeOnly.FromDateTime(DateTime.Now)
                    };
                    _context.StockUnitCostHistory.Add(StockUnitCostHistory);
                }
                else if (StockUnitCostHistory.CostPrice != StockUnitPrice)
                {
                    // ถ้าราคาใหม่สูงกว่าราคาเดิม ให้สร้างรายการใหม่
                    var newCostHistory = new StockUnitCostHistory
                    {
                        StockId = StockId,
                        CostPrice = StockUnitPrice,
                        EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
                        CreatedBy = UpdateBy,
                        CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                        CreatedTime = TimeOnly.FromDateTime(DateTime.Now)
                    };
                    _context.StockUnitCostHistory.Add(newCostHistory);
                }
                else if (StockUnitCostHistory.CostPrice == StockUnitPrice)
                {
                    // ถ้าไม่มีการเปลี่ยนแปลงราคา ไม่ต้องทำอะไร
                    return;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Inner: " + ex.InnerException?.Message);
                throw; // หรือจะ return BadRequest ก็ได้
            }    
        }
    }
}