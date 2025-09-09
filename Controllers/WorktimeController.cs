using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class WorktimeController : ControllerBase
    {
        private readonly IWorktimeService _WorktimeService;
        private readonly ILogger<WorktimeService> _logger;
        public WorktimeController(IWorktimeService WorktimeService, ILogger<WorktimeService> logger)
        {
            _WorktimeService = WorktimeService;
            _logger = logger;
        }

        [HttpPost("ClockIn")]
        public async Task<IActionResult> ClockIn(WorktimeDto WorktimeDto)
        {
            try
            {
                var result = await _WorktimeService.ClockIn(WorktimeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 Error : " + ex.Message);
                return BadRequest("เกิดปัญหาในการลงเวาเข้างาน");
            }
        }
        [HttpPost("ClockOut")]
        public async Task<IActionResult> ClockOut(WorktimeDto WorktimeDto)
        {
            try
            {
                var result = await _WorktimeService.ClockOut(WorktimeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 Error : " + ex.Message);
                return BadRequest("เกิดปัญหาในการลงเวาเข้างาน");
            }
        }
        [HttpPost("GetPeriodWorktimeByEmployeeID")]
        public async Task<IActionResult> GetPeriodWorktimeByEmployeeID(WorktimeDto WorktimeDto)
        {
            try
            {
                var result = await _WorktimeService.GetPeriodWorktimeByEmployeeID(WorktimeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 Error : " + ex.Message);
                return BadRequest("เกิดปัญหาในการดึงข้อมูลเวลาทำงาน");
            }
        }

        [HttpPost("GetWorkTimeHistoryByEmployeeID")]
        public async Task<IActionResult> GetWorkTimeHistoryByEmployeeID(WorktimeDto WorktimeDto)
        {
            try
            {
                var result = await _WorktimeService.GetWorkTimeHistoryByEmployeeID(WorktimeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 Error : " + ex.Message);
                return BadRequest("เกิดปัญหาในการดึงข้อมูลประวัติการทำงาน");
            }
        }
        [HttpPost("GetWorkTimeHistoryByPeriod")]
        public async Task<IActionResult> GetWorkTimeHistoryByPeriod(WorktimeDto WorktimeDto)
        {
            try
            {
                var result = await _WorktimeService.GetWorkTimeHistoryByPeriod(WorktimeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 Error : " + ex.Message);
                return BadRequest("เกิดปัญหาในการดึงข้อมูลประวัติการทำงาน");
            }
        }
        [HttpPost("GetWorkTimeCostByEmployeeIDandPeriod")]
        public async Task<IActionResult> GetWorkTimeCostByEmployeeIDandPeriod(WorktimeDto worktimeDto)
        {
            try
            {
                var result = await _WorktimeService.GetWorkTimeCostByEmployeeIDandPeriod(worktimeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 Error : " + ex.Message);
                return BadRequest("เกิดปัญหาในการดึงข้อมูลต้นทุนการทำงาน");
            }
        }
    }
}