using chickko.api.Dtos;
using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IStatementService
    {
        Task InsertIncomeAsync(IncomeStatementDto incomeStatementDto);
        Task InsertStatementAsync(StatementDto statementDto);
        Task<List<IncomeType>> GetIncomeTypesAsync();
    }
}