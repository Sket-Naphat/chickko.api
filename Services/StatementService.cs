using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services
{

    public class StatementService : IStatementService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<StatementService> _logger;
        private readonly IUtilService _utilService;
        public StatementService(ChickkoContext context, ILogger<StatementService> logger, IUtilService utilService)
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
        }


        public async Task InsertIncomeAsync(IncomeStatementDto income)
        {  
            try
            {
                Income InsertIncome = new Income
                {
                    IncomeDate = income.IncomeDate,
                    IncomeValue = income.IncomeValue,
                    IncomeTime = income.IncomeTime,
                    IncomeTypeId = income.IncomeTypeId,
                    UserId = income.UserId,
                    IncomeDescription = income.IncomeDescription,
                    UpdateDate = _utilService.GetThailandDate(),
                    UpdateTime = _utilService.GetThailandTime()
                };
                _context.Incomes.Add(InsertIncome);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting income");
                throw;
            }
            
        }

        public async Task InsertStatementAsync(StatementDto statementDto)
        {
            try
            {
                Statement InsertStatement = new Statement
                {
                    StatementDate = statementDto.StatementDate,
                    StatementValue = statementDto.StatementValue,
                    StatementTime = statementDto.StatementTime,
                    StatementDescription = statementDto.StatementDescription,
                    UserId = statementDto.UserId,
                    UpdateDate = _utilService.GetThailandDate(),
                    UpdateTime = _utilService.GetThailandTime()
                };
                _context.Statements.Add(InsertStatement);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting statement");
                throw;
            }
        }
        public async Task<List<IncomeType>> GetIncomeTypesAsync()
        {
            try
            {
                var incomeTypes = await _context.IncomeTypes.Where(it => it.Active).ToListAsync();
                return incomeTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving income types");
                throw;
            }
        }
    }
}