using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class CostController : ControllerBase
    {
        private readonly ICostService _costService;
        private readonly IUtilService _utilService;
        public CostController(ICostService costService, IUtilService utilService)
        {
            _costService = costService;
            _utilService = utilService;
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
        [HttpPost("GetWageCostList")]
        public async Task<IActionResult> GetWageCostList()
        {
            try
            {
                var result = await _costService.GetWageCostList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("CreateCost")]
        public async Task<IActionResult> CreateCost(CostDto costDto)
        {
            try
            {
                var _cost = new Cost
                {
                    CostCategoryID = costDto.CostCategoryID,
                    CostPrice = costDto.CostPrice,
                    CostDescription = costDto.CostDescription,
                    CostDate = costDto.CostDate ?? DateOnly.FromDateTime(System.DateTime.Now),
                    CostTime = costDto.CostTime ?? TimeOnly.FromDateTime(System.DateTime.Now),
                    IsPurchese = costDto.IsPurchese,
                    PurcheseDate = DateOnly.FromDateTime(System.DateTime.Now),
                    PurcheseTime = TimeOnly.FromDateTime(System.DateTime.Now),
                    CostStatusID = costDto.CostStatusID,
                };
                await _costService.CreateCost(_cost);
                return Ok();
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    Message = ex.Message
                };
                await _utilService.AddErrorLog(errorLog);
                return BadRequest("เกิดข้อผิดพลาดในการเพิ่มต้นทุน");

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