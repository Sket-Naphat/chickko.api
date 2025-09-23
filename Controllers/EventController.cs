using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using chickko.api.Interface;
using chickko.api.Services.Event;
using chickko.api.Dtos.Event;

namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventRollingService _rollingService;

        public EventController(ILogger<EventController> logger, IEventRollingService rollingService)
        {
            _logger = logger;
            _rollingService = rollingService;
        }

        [HttpGet("getRollingRewardList")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRollingRewardList()
        {
            try
            {
                var rewardList = await _rollingService.GetRollingRewardList();

                return Ok(new { 
                    success = true, 
                    data = rewardList,
                    message = "ดึงข้อมูลรายการรางวัลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูลรายการรางวัล");
                return StatusCode(500, new { 
                    success = false, 
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์" 
                });
            }
        }
    }
}