using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IOrdersService
    {
        Task<string> CopyOrderFromFirestore();
        Task<string> ImportOrderFromExcel();

    }
}