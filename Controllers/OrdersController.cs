using Microsoft.AspNetCore.Mvc;
using chickko.api.Services;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization;

namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }
        [HttpPost("CopyOrderFromFirestore")]

        public async Task<IActionResult> CopyOrderFromFirestore(CopyOrderFromFirestore _CopyOrderFromFirestore)
        {
            var result = await _ordersService.CopyOrderFromFirestore(_CopyOrderFromFirestore.OrderDateFrom.ToString() ?? "", _CopyOrderFromFirestore.OrderDateTo.ToString() ?? "");
            return Ok(result);
        }
        [HttpPost("ImportOrderFromExcel")]
        public async Task<IActionResult> ImportOrderFromExcel()
        {
            var result = await _ordersService.ImportOrderFromExcel();
            return Ok(result);
        }
    }
}