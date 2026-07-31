using chickko.api.Dtos;
using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IStatementService
    {
        Task InsertIncomeAsync(IncomeStatementDto incomeStatementDto);
        Task InsertStatementAsync(StatementDto statementDto);
        Task<List<IncomeType>> GetIncomeTypesAsync();
        Task<List<IncomeStatementDto>> GetIncomeAsync(DateOnly? dateFrom, DateOnly? dateTo, int? incomeTypeId = null);
        Task UpdateIncomeAsync(IncomeStatementDto incomeStatementDto);
        Task DeleteIncomeAsync(int incomeId);
        Task<DailyReportSummaryDto> GetStatementSummaryAsync(SaleDateDto saleDateDto);
    }
}