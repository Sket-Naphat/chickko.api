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
        [HttpGet("GetCostCategoryList")]
        public async Task<IActionResult> GetCostCategoryList()
        {
            try
            {
                var result = await _costService.GetCostCategoryList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("GetAllCostList")]
        public async Task<IActionResult> GetAllCostList(GetCostListDto getCostListDto)
        {
            try
            {
                var result = await _costService.GetAllCostList(getCostListDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("GetCostListReport")]
        public async Task<IActionResult> GetCostListReport(GetCostListDto getCostListDto)
        {
            try
            {
                var result = await _costService.GetCostListReport(getCostListDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //new cost
        [HttpPost("GetStockCostRequest")]
        public async Task<IActionResult> GetStockCostRequest(CostDto costDto)
        {
            try
            {
                var result = await _costService.GetStockCostRequest(costDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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
                    IsPurchase = costDto.IsPurchase,
                    CostStatusID = costDto.CostStatusID == null ? (costDto.IsPurchase ? 3 : 2) : costDto.CostStatusID, //ถ้าไม่มีค่า CostStatusID ให้ใช้ค่า IsPurchase แทน
                    CreateBy = costDto.UpdateBy ?? 0,
                    CreateDate = costDto.CreateDate ?? DateOnly.FromDateTime(System.DateTime.Now),
                    CreateTime = costDto.CreateTime ?? TimeOnly.FromDateTime(System.DateTime.Now),
                    IsActive = true
                };
                if (costDto.IsPurchase)
                {
                    _cost.PurchaseDate = costDto.PurchaseDate ?? DateOnly.FromDateTime(System.DateTime.Now);
                    _cost.PurchaseTime = costDto.PurchaseTime ?? TimeOnly.FromDateTime(System.DateTime.Now);
                }
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
        [HttpPost("UpdatePurchaseCost")]
        public async Task<IActionResult> UpdatePurchaseCost(CostDto costDto)
        {
            try
            {
                var _cost = new Cost
                {
                    CostId = costDto.CostID,
                    CostCategoryID = costDto.CostCategoryID,
                    CostPrice = costDto.CostPrice,
                    CostDescription = costDto.CostDescription,
                    IsPurchase = costDto.IsPurchase,
                    PurchaseDate = costDto.PurchaseDate ?? DateOnly.FromDateTime(System.DateTime.Now),
                    PurchaseTime = costDto.PurchaseTime ?? TimeOnly.FromDateTime(System.DateTime.Now),
                    CostStatusID = costDto.CostStatusID == null ? (costDto.IsPurchase ? 3 : 2) : costDto.CostStatusID, //ถ้าไม่มีค่า CostStatusID ให้ใช้ค่า IsPurchase แทน
                    UpdateBy = costDto.UpdateBy ?? 0, // ถ้า UpdateBy เป็น null ให้ใช้ค่า 0
                    UpdateDate = costDto.UpdateDate ?? DateOnly.FromDateTime(System.DateTime.Now),
                    UpdateTime = costDto.UpdateTime ?? TimeOnly.FromDateTime(System.DateTime.Now)
                };
                await _costService.UpdatePurchaseCost(_cost);
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

        [HttpPost("UpdateWageCost")]
        public async Task<IActionResult> UpdateWageCost(WorktimeDto worktimeDto)
        {
            try
            {
                await _costService.UpdateWageCost(worktimeDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //delete cost
        [HttpDelete("DeleteCost/{id}")]
        public async Task<IActionResult> DeleteCost(int id)
        {
            try
            {
                var result = await _costService.DeleteCost(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}