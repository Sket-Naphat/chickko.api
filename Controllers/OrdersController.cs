using Microsoft.AspNetCore.Mvc;
using chickko.api.Interface;
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
        private readonly IMenuService _menusService;

        public OrdersController(IOrdersService ordersService, IMenuService menusService)
        {
            _ordersService = ordersService;
            _menusService = menusService;
        }
        [HttpPost("CopyOrderFromFirestore")]

        public async Task<IActionResult> CopyOrderFromFirestore(CopyOrderFromFirestore _CopyOrderFromFirestore)
        {
            // await _menusService.CopyMenusFromFirestore();
            var result = await _ordersService.CopyOrderFromFirestore();
            return Ok(new
            {
                success = true,
                message = result
            });
        }
        [HttpPost("ImportOrderFromExcel")]
        public async Task<IActionResult> ImportOrderFromExcel()
        {
            var result = await _ordersService.ImportOrderFromExcel();
            return Ok(new
            {
                success = true,
                message = result
            });
        }
    }
}