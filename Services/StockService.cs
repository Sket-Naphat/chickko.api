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
                    .Where(s => s.Active == true )
                    .ToListAsync();

                var stockDtos = stocks.Select(s => new StockDto
                {
                    StockId = s.StockId,
                    ItemName = s.ItemName,
                    StockCategoryID = s.StockCategoryID,
                    StockCategoryName = s.StockCategory!.StockCategoryName ,
                    StockUnitTypeID = s.StockUnitTypeID,
                    StockUnitTypeName = s.StockUnitType!.StockUnitTypeName ,
                    StockLocationID = s.StockLocationID,
                    StockLocationName = s.StockLocation!.StockLocationName ,
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

        public async Task<StockLog> CreateStockCountLog(StockCountDto stockCountDto, int costId)
        {
            try
            {
                var stock = await _context.Stocks.FindAsync(stockCountDto.StockId)
                ?? throw new Exception($"ไม่พบ Stock ID: {stockCountDto.StockId}");

                // 2. แปลงวันที่และเวลาให้อยู่ในรูปแบบ DateOnly และ TimeOnly
                //var date = DateOnly.Parse(stockCountDto.StockInDate);
                var date = DateOnly.TryParseExact(stockCountDto.StockInDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                var time = TimeOnly.Parse(stockCountDto.StockInTime);
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
                    StockInDate = date,
                    StockInTime = time,
                    TotalQTY = stockCountDto.TotalQTY, //จำนวนคงเหลือ
                    RequiredQTY = RequiredQTY, //จนวนที่ต้องใช้
                    StockInQTY = StockInQTY, //จำนวนที่ต้องซื้อ
                    StockLogTypeID = 1, // 1 = Count (ขานับ)
                    Remark = stockCountDto.Remark,
                    CostId = costId
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
        public async Task CreateStocInLog(StockInDto stockInDto)
        {
            // 1. ดึง Stock หลักจาก DB โดยใช้ StockId ที่ผู้ใช้ส่งมา
            var stock = await _context.Stocks.FindAsync(stockInDto.StockId)
                ?? throw new Exception($"ไม่พบ Stock ID: {stockInDto.StockId}");

            // 2. แปลงวันที่และเวลาให้อยู่ในรูปแบบ DateOnly และ TimeOnly
            var date = DateOnly.TryParseExact(stockInDto.StockInDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
            var time = TimeOnly.Parse(stockInDto.StockInTime);

            int DipQTY = stockInDto.PurchaseQTY - stockInDto.StockInQTY; //จำนวนที่สั่ง กับที่ซื้อจีิง

            int StockInQTY = stockInDto.StockInQTY - stockInDto.PurchaseQTY;
            StockInQTY = (StockInQTY < 0) ? 0 : StockInQTY;

            // 3. สร้าง StockLog ใหม่สำหรับบันทึกการ "ซื้อเข้า"
            var stockLog = new StockLog
            {
                StockId = stock.StockId,
                StockInDate = date,
                StockInTime = time,
                StockInQTY = stockInDto.StockInQTY, //จำนวนที่ต้องซื้อเพิ่ม
                PurchaseQTY = stockInDto.PurchaseQTY,
                DipQTY = DipQTY,
                Price = stockInDto.Price,
                SupplyID = stockInDto.SupplyId,
                RequiredQTY = stock.RequiredQTY, // จำนวนที่ต้องการ
                TotalQTY = stock.TotalQTY + stockInDto.PurchaseQTY, // คำนวณยอดคงเหลือใหม่
                StockLogTypeID = 2, // 2 = Purchase (ซื้อเข้า)
                Remark = stockInDto.Remark,
                IsPurchase = true
            };

            // 4. บันทึก StockLog ลงฐานข้อมูล
            _context.StockLogs.Add(stockLog);
            await _context.SaveChangesAsync();

            // 5. อัปเดต Stock หลักให้เพิ่มจำนวนที่ซื้อเข้ามา
            stock.StockInQTY = StockInQTY; //หักกับจำนวนที่ซื้อเข้ามาแล้ว
            stock.TotalQTY = stockLog.TotalQTY;
            stock.UpdateDate = date;
            stock.UpdateTime = time;
            stock.RecentStockLogId = stockLog.StockLogId;

            // 6. บันทึกการอัปเดต Stock
            await _context.SaveChangesAsync();
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
    }
}