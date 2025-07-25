using chickko.api.Dtos;

namespace chickko.api.Interface
{
    public interface ICostService
    {
        Task<string> addNewCost(CostDto costDto);
    }
}