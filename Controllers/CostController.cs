using chickko.api.Dtos;
using chickko.api.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class CostController : ControllerBase
    {
        private readonly ICostService _costService;
        public CostController(ICostService costService)
        {
            _costService = costService;
        }
        //new cost
        [HttpPost("AddNewCost")]
        public async Task<IActionResult> AddNewCost(CostDto costDto)
        {
            try
            {
                var result = await _costService.addNewCost(costDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}