using Microsoft.AspNetCore.Mvc;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization;
using chickko.api.Dtos;

namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // เพิ่มการตรวจสอบสิทธิ์
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly IMenuService _menusService;
        private readonly ICostService _costService;

        public OrdersController(IOrdersService ordersService, IMenuService menusService, ICostService costService)
        {
            _ordersService = ordersService;
            _menusService = menusService;
            _costService = costService;
        }
        [HttpPost("CopyOrderFromFirestore")]

        public async Task<IActionResult> CopyOrderFromFirestore(CopyOrderFromFirestore _CopyOrderFromFirestore)
        {
            // await _menusService.CopyMenusFromFirestore();
            var result = await _ordersService.CopyOrderFromFirestore();
            return Ok(new
            {
                success = true,
                message = result
            });
        }
        [HttpPost("ImportOrderFromExcel")]
        public async Task<IActionResult> ImportOrderFromExcel()
        {
            var result = await _ordersService.ImportOrderFromExcel();
            return Ok(new
            {
                success = true,
                message = result
            });
        }
        [HttpPost("GetDailyDineInSalesReport")]
        public async Task<IActionResult> GetDailyDineInSalesReport(SaleDateDto saleDateDto)
        {
            var result = await _ordersService.GetDailyDineInSalesReport(saleDateDto);
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpPost("GetDailyDeliverySalesReport")]
        public async Task<IActionResult> GetDailyDeliverySalesReport(SaleDateDto saleDateDto)
        {
            var result = await _ordersService.GetDailyDeliverySalesReport(saleDateDto);
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpPost("GetDailyReport")]
        public async Task<IActionResult> GetDailyReport(SaleDateDto saleDateDto)
        {
            var dineInResult = await _ordersService.GetDailyDineInSalesReport(saleDateDto);
            var deliveryResult = await _ordersService.GetDailyDeliverySalesReport(saleDateDto);
            var GetSaleOfMenu = await _ordersService.GetSaleOfMenu(saleDateDto.Year ?? 0, saleDateDto.Month ?? 0);

            GetCostListDto getCostListDto = new GetCostListDto
            {
                Month = saleDateDto.Month,
                Year = saleDateDto.Year,
                IsPurchase = saleDateDto.IsPurchase
            };
            var dailyCostReportDto = await  _costService.GetCostListReport(getCostListDto);

            DashboardDto dashboardDto = new DashboardDto
            {
                Year = saleDateDto.Year ?? 0,
                Month = saleDateDto.Month ?? 0,
                GetSaleOfMenu = GetSaleOfMenu,
                DailyDineInSalesReport = dineInResult,
                DailyDeliverySalesReport = deliveryResult,
                DailyCostReport = dailyCostReportDto
            };
            return Ok(new
            {
                success = true,
                data = dashboardDto
            });
        }

        [HttpPost("UpdateDeliveryRecords")]
        public async Task<IActionResult> UpdateDeliveryRecords(DeliveryDto deliveryDto)
        {
            var result = await _ordersService.UpdateDeliveryRecords(deliveryDto);
            return Ok(new
            {
                success = true,
                message = result
            });
        }
        [HttpPost("GetDeliveryRecords")]
        public async Task<IActionResult> GetDeliveryRecords(DeliveryDto deliveryDto)
        {
            var result = await _ordersService.GetDeliveryRecords(deliveryDto);
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpPost("GetDeliveryOrdersByDate")]
        public async Task<IActionResult> GetDeliveryOrdersByDate(DeliveryDto deliveryDto)
        {
            var result = await _ordersService.GetDeliveryOrdersByDate(deliveryDto);
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpPost("GetIncomeOrdersByDate")]
        public async Task<IActionResult> GetIncomeOrdersByDate(IncomeDto incomeDto)
        {
            var result = await _ordersService.GetIncomeOrdersByDate(incomeDto);
            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }
}