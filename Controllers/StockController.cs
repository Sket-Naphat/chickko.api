using System.Globalization;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ICostService _costService;
        public StockController(IStockService stockService, ICostService costService)
        {
            _stockService = stockService;
            _costService = costService;
        }
        [HttpGet("GetAllStockItem")]
        public async Task<IActionResult> GetAllStockItem()
        {
            try
            {
                var result = await _stockService.GetAllStockItem();
                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "โหลดข้อมูลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Controller GetAllStockItem: {ex.Message}");
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดขณะดึงข้อมูลสต็อก โปรดแจ้งพี่สเก็ต" });
            }
        }
        [HttpGet("GetCurrentStock")]
        public async Task<IActionResult> GetCurrentStock()
        {
            try
            {
                var result = await _stockService.GetCurrentStock();
                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "โหลดข้อมูลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Controller GetCurrentStock: {ex.Message}");
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดขณะดึงข้อมูลสต็อก โปรดแจ้งพี่สเก็ต" });
            }
        }

        [HttpPost("CreateStockCount")]
        public async Task<IActionResult> CreateStockCount([FromBody] List<StockCountDto> stockCountDto)
        {
            var successList = new List<int>();
            var failedList = new List<object>();
            var _stockLog = new StockLog();
            var firstStock = stockCountDto.FirstOrDefault();
            var costDate = DateOnly.FromDateTime(DateTime.Now);
            if (firstStock != null)
            {
                costDate = DateOnly.TryParseExact(firstStock.StockCountDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
            }

            var nowDate = DateOnly.FromDateTime(DateTime.Now);
            var nowTime = TimeOnly.FromDateTime(DateTime.Now);
            var addCost = new Cost
            {
                CostCategoryID = 1,
                CostPrice = 0,
                CostDate = costDate,
                CostTime = nowTime,
                UpdateDate = nowDate,
                UpdateTime = nowTime,
                IsPurchase = false,
                CostStatusID = 1,
                CreateBy = stockCountDto.FirstOrDefault()?.UpdateBy ?? 0,
                CreateDate = nowDate,
                CreateTime = nowTime,
                CostDescription = "นับ Stock รายวัน"
            };

            var createdCost = await _costService.CreateCostReturnCostID(addCost); // ต้อง return ค่า Cost จาก service
            var createdCostId = createdCost.CostId;

            foreach (var stock in stockCountDto)
            {
                try
                {

                    await _stockService.CreateStockCountLog(stock, createdCostId);
                    successList.Add(stock.StockId);
                }
                catch (Exception ex)
                {
                    failedList.Add(new
                    {
                        StockId = stock.StockId,
                        Error = ex.Message
                    });
                }
            }

            return Ok(new
            {
                message = "ผลลัพธ์การบันทึกข้อมูล",
                successCount = successList.Count,
                failedCount = failedList.Count,
                successStockIds = successList,
                failedItems = failedList
            });
        }
        [HttpPost("UpdateStockCount")]
        public async Task<IActionResult> UpdateStockCount(UpdateStockCountDto updateStockCountDto)
        {
            try
            {
                var costDate = updateStockCountDto.StockCountDate;
                var costId = updateStockCountDto.CostID;
                var updateBy = updateStockCountDto.UpdateBy;
                //update cost
                if (costId != 0)
                {
                    await _costService.UpdateStockCostDate(costDate, costId, updateBy);
                }

                await _stockService.UpdateStockCountLog(updateStockCountDto.StockCountDto, updateStockCountDto.CostID);
                return Ok(new
                {
                    success = true,
                    message = "โหลดข้อมูลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Controller GetCurrentStock: {ex.Message}");
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดขณะดึงข้อมูลสต็อก โปรดแจ้งพี่สเก็ต" });
            }
        }
        [HttpPost("CreateStockIn")]
        public async Task<IActionResult> CreateStockIn(UpdateStockInCostDto updateStockInCostDto)
        {
            var successList = new List<int>();
            var failedList = new List<object>();
            var IsPurchase = updateStockInCostDto.UpdateStockCostDto.IsPurchase;
            var UpdateBy = updateStockInCostDto.UpdateStockCostDto.UpdateBy;

            foreach (var stock in updateStockInCostDto.StockInDto)
            {
                try
                {
                    stock.IsPurchase = IsPurchase;
                    stock.UpdateBy = UpdateBy;
                    await _stockService.CreateStockIn(stock , updateStockInCostDto.UpdateStockCostDto.CostID);
                    successList.Add(stock.StockId);
                }
                catch (Exception ex)
                {
                    failedList.Add(new
                    {
                        StockId = stock.StockId,
                        Error = ex.Message
                    });
                }
            }
            // Update cost information
            await _costService.UpdateStockCost(updateStockInCostDto.UpdateStockCostDto);

            return Ok(new
            {
                message = "ผลลัพธ์การบันทึกข้อมูล",
                successCount = successList.Count,
                failedCount = failedList.Count,
                successStockIds = successList,
                failedItems = failedList
            });
        }
        [HttpPost("CreateStockDetail")]
        public async Task<IActionResult> CreateStockDetail(StockDto stockDto)
        {
            await _stockService.CreateStockDetail(stockDto);
            return Ok();
        }
        [HttpPost("UpdateStockDetail")]
        public async Task<IActionResult> UpdateStockDetail(StockDto stockDto)
        {
            await _stockService.UpdateStockDetail(stockDto);
            return Ok();
        }
        [HttpPost("GetStockCountLogByCostId")]
        public async Task<IActionResult> GetStockCountLogByCostId([FromBody] StockInDto stockCountDto)
        {
            try
            {
                var result = await _stockService.GetStockCountLogByCostId(stockCountDto);
                if (result == null || result.StockCountDtos.Count == 0)
                {
                    return NotFound(new { message = "ไม่พบข้อมูลการนับสต็อกสำหรับ Cost ID ที่ระบุ" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetStockUnitType")]
        public async Task<IActionResult> GetStockUnitType()
        {
            try
            {
                var result = await _stockService.GetStockUnitType();
                if (result == null || result.Count == 0)
                {
                    return NotFound(new { message = "ไม่พบข้อมูลหมวดหมู่สินค้า" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("GetStockLocation")]
        public async Task<IActionResult> GetStockLocation()
        {
            try
            {
                var result = await _stockService.GetStockLocation();
                if (result == null || result.Count == 0)
                {
                    return NotFound(new { message = "ไม่พบข้อมูลสถานที่จัดเก็บสินค้า" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("GetStockCategory")]
        public async Task<IActionResult> GetStockCategory()
        {
            try
            {
                var result = await _stockService.GetStockCategory();
                if (result == null || result.Count == 0)
                {
                    return NotFound(new { message = "ไม่พบข้อมูลหมวดหมู่สินค้า" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}