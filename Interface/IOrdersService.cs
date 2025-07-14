using chickko.api.Models;

namespace chickko.api.Services
{
    public interface IOrdersService
    {
        Task<string> CopyOrderFromFirestore(string datefrom = "", string dateto = "");

    }
}