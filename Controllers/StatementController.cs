using chickko.api.Dtos;
using chickko.api.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chickko.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class StatementController : ControllerBase
    {
        private readonly ILogger<StatementController> _logger;
        private readonly IStatementService _statementService;

        public StatementController(ILogger<StatementController> logger, IStatementService statementService)
        {
            _logger = logger;
            _statementService = statementService;
        }

        [HttpPost("CreateIncome")]
        public async Task<IActionResult> CreateIncome(IncomeStatementDto incomeStatementDto)
        {
            try
            {
                await _statementService.InsertIncomeAsync(incomeStatementDto);

                return Ok(new { message = "Income created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating income");
                return StatusCode(500, "An error occurred while creating income");
            }
        }
        [HttpGet("GetIncome")]
        public async Task<IActionResult> GetIncome([FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo)
        {
            try
            {
                var income = await _statementService.GetIncomeAsync(dateFrom, dateTo);

                return Ok(new { data = income, message = "Income retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving income");
                return StatusCode(500, "An error occurred while retrieving income");
            }
        }

        [HttpGet("GetIncomeType")]
        public async Task<IActionResult> GetIncomeType()
        {
            try
            {
                var incomeTypes = await _statementService.GetIncomeTypesAsync();
                // Implementation for getting income types goes here

                return Ok(new { data = incomeTypes , message = "Income types retrieved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving income types");
                return StatusCode(500, "An error occurred while retrieving income types");
            }
        }

        [HttpPut("UpdateIncome")]
        public async Task<IActionResult> UpdateIncome(IncomeStatementDto incomeStatementDto)
        {
            try
            {
                await _statementService.UpdateIncomeAsync(incomeStatementDto);
                return Ok(new { message = "Income updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating income");
                return StatusCode(500, "An error occurred while updating income");
            }
        }

        [HttpDelete("DeleteIncome")]
        public async Task<IActionResult> DeleteIncome([FromQuery] int incomeId)
        {
            try
            {
                await _statementService.DeleteIncomeAsync(incomeId);
                return Ok(new { message = "Income deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting income");
                return StatusCode(500, "An error occurred while deleting income");
            }
        }
    }
}