using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IOrdersService
    {
        Task<string> CopyOrderFromFirestore();
        Task<string> ImportOrderFromExcel();
        Task<List<DailySaleDto>> GetDailyDineInSalesReport(DateOnly date);
        Task<string> UpdateDeliveryRecords(DeliveryDto deliveryDto);
        Task<List<DeliveryDto>> GetDeliveryRecords(DeliveryDto deliveryDto);
        Task<List<DeliveryOrdersDTO>> GetDeliveryOrdersByDate(DeliveryDto deliveryDto);
    }
}