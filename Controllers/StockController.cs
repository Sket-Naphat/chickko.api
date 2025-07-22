using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IStockService _stockService;
        public OrdersController(IStockService stockService)
        {
            _stockService = stockService;
        }
        [HttpGet("GetStock")]
         public async Task<IActionResult> GetStock()
        {
            var result = await _stockService.GetStock();
            return Ok(result);
        }

    }
}