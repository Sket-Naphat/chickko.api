using Microsoft.AspNetCore.Mvc;
using chickko.api.Services;
using chickko.api.Models;

namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        // Example: GET /api/orders
        // [HttpGet]
        // public IActionResult GetAllOrders()
        // {
        //     var orders = _ordersService.GetAllOrders();
        //     return Ok(orders);
        // }

        // // Example: GET /api/orders/{id}
        // [HttpGet("{id}")]
        // public IActionResult GetOrderById(int id)
        // {
        //     var order = _ordersService.GetOrderById(id);
        //     if (order == null)
        //         return NotFound();
        //     return Ok(order);
        // }

        // // Example: POST /api/orders
        // [HttpPost]
        // public IActionResult CreateOrder([FromBody] OrderDto orderDto)
        // {t5
        //     var createdOrder = _ordersService.CreateOrder(orderDto);
        //     return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
        // }
        [HttpPost("CopyOrderFromFirestore")]
        public async Task<IActionResult> CopyOrderFromFirestore(CopyOrderFromFirestore _CopyOrderFromFirestore)
        {
            var result = await _ordersService.CopyOrderFromFirestore(_CopyOrderFromFirestore.OrderDateFrom.ToString() ?? "", _CopyOrderFromFirestore.OrderDateTo.ToString()?? "");
            return Ok(result);
        }
    }
}