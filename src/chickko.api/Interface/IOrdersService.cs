using chickko.api.Dtos;
using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IOrdersService
    {
        Task<string> CopyOrderFromFirestore();
        Task<string> ImportOrderFromExcel();
        Task<List<DailySaleDto>> GetDailyDineInSalesReport(SaleDateDto saleDateDto);
        Task<List<DailySaleDto>> GetDailyDeliverySalesReport(SaleDateDto saleDateDto);
        Task<string> UpdateDeliveryRecords(DeliveryDto deliveryDto);
        Task<List<DeliveryDto>> GetDeliveryRecords(DeliveryDto deliveryDto);
        Task<List<DeliveryOrdersDTO>> GetDeliveryOrdersByDate(DeliveryDto deliveryDto);
        Task<List<IncomeOrdersDTO>> GetIncomeOrdersByDate(IncomeDto incomeDto);
        Task<List<CategorySaleDto>> GetSaleOfMenu(int year, int month);
    }
}