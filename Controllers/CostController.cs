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
    public class CostController : ControllerBase
    {
        private readonly ICostService _costService;
        public CostController(ICostService costService)
        {
            _costService = costService;
        }
        //new cost
        [HttpPost("GetStockCostList")]
        public async Task<IActionResult> GetStockCostList(CostDto costDto)
        {
            try
            {
                var result = await _costService.GetStockCostList(costDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("UpdateStockCost")]
        public async Task<IActionResult> UpdateStockCost(UpdateStockCostDto updateStockCostDto)
        {
            try
            {
                await _costService.UpdateStockCost(updateStockCostDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}