using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }
        [HttpGet("GetCurrentStock")]
        public async Task<IActionResult> GetCurrentStock()
        {
            try
            {
                var result = await _stockService.GetCurrentStock();
                return Ok(result);
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

            foreach (var stock in stockCountDto)
            {
                try
                {
                    await _stockService.CreateStockCountLog(stock);
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
        [HttpPost("CreateStocIn")]
        public async Task<IActionResult> CreateStocIn([FromBody] List<StockInDto> stockInDto)
        {
            var successList = new List<int>();
            var failedList = new List<object>();

            foreach (var stock in stockInDto)
            {
                try
                {
                    await _stockService.CreateStocInLog(stock);
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

    }
}