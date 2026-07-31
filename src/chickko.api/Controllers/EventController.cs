using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using chickko.api.Interface;
using chickko.api.Services.Event;
using chickko.api.Dtos.Event;
using System.Globalization;

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

                return Ok(new
                {
                    success = true,
                    data = rewardList,
                    message = "ดึงข้อมูลรายการรางวัลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูลรายการรางวัล");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
        [HttpPost("saveRollingGameReward")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveRollingGameReward([FromBody] RollingResultDto resultDto)
        {
            try
            {
                await _rollingService.SaveRollingGameReward(resultDto);

                return Ok(new
                {
                    success = true,
                    data = resultDto,
                    message = "บันทึกรายการรางวัลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการบันทึกรายการรางวัล");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
        [HttpGet("gethistoryrollinggame")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHistoryRollingGame([FromQuery] string OrderFirstStoreID)
        {
            try
            {
                var history = await _rollingService.GetHistoryRollingGame(OrderFirstStoreID);

                return Ok(new
                {
                    success = true,
                    data = history,
                    message = "ดึงข้อมูลประวัติการเล่นเกมสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูลประวัติการเล่นเกม");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
        [HttpPost("addRollingReward")]
        public async Task<IActionResult> AddRollingReward([FromBody] RollingRewardDto rewardDto)
        {
            try
            {
                await _rollingService.AddRollingReward(rewardDto);

                return Ok(new
                {
                    success = true,
                    data = rewardDto,
                    message = "เพิ่มรางวัลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มรางวัล");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
        [HttpPut("updateRollingReward")]
        public async Task<IActionResult> UpdateRollingReward([FromBody] RollingRewardDto rewardDto)
        {
            try
            {
                await _rollingService.UpdateRollingReward(rewardDto);

                return Ok(new
                {
                    success = true,
                    data = rewardDto,
                    message = "อัปเดตรางวัลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการอัปเดตรางวัล");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
        [HttpDelete("deleteRollingReward/{rollingRewardId}")]
        public async Task<IActionResult> DeleteRollingReward(int rollingRewardId)
        {
            try
            {
                await _rollingService.DeleteRollingReward(rollingRewardId);

                return Ok(new
                {
                    success = true,
                    message = "ลบรางวัลสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบรางวัล");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
        [HttpGet("getRollingGameReport")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRollingGameReport([FromQuery] string? date)
        {
            try
            {
                // แปลงวันที่จาก query string
                DateOnly? reportDate = null;
                if (!string.IsNullOrEmpty(date) && DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    reportDate = parsedDate;
                }

                // ดึงข้อมูลจาก service (ส่งวันที่ถ้ามี)
                var report = await _rollingService.GetRollingGameReport(reportDate);

                return Ok(new
                {
                    success = true,
                    data = report,
                    message = "ดึงรายงานสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายงาน Rolling Game");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
                });
            }
        }
    }
}