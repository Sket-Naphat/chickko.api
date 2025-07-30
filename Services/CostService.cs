using System.Globalization;
using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services
{
    public class CostService : ICostService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<StockService> _logger;
        public CostService(ChickkoContext context, ILogger<StockService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task CreateCost(Cost _Cost)
        {
            try
            {
                _context.Cost.Add(_Cost);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EF Core SaveChanges Error: {Message}", ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        }

        public async Task<Cost> CreateCostReturnCostID(Cost cost)
        {
            _context.Cost.Add(cost);
            await _context.SaveChangesAsync();
            return cost;
        }

        #region Stock Cost
        public async Task<List<StockDto>> GetStockCostList(CostDto costDto) //http://localhost:5036/api/stock/GetStockCostList
        {
            try
            {
                //หาก่อนว่าในวันนี้มีรายการที่ต้องสั่งซื้อมั้ย
                //var _cost = await _context.Cost.Where(c => c.CostDate == costDto.CostDate && !c.IsPurchese).ToListAsync();

                var query = _context.Cost.Where(c => !c.IsPurchese && c.CostCategoryID == 1);

                if (costDto is { CostDate: not null and var date } && date != default)
                {
                    query = query.Where(c => c.CostDate == date);
                }

                var _cost = await query.ToListAsync();

                var StockDto = new List<StockDto>();
                if (_cost != null && _cost.Count > 0)
                {
                    //ถ้ามีที่ยังไม่จ่ายเงินทีให้ทำการ get stock และราคา
                    var _stock = await _context.Stock
                                .Include(s => s.StockCategory)
                                .Include(s => s.StockUnitType)
                                .Include(s => s.StockLocation)
                                .Where(s => s.Active == true && s.StockInQTY > 0)
                                .ToListAsync();

                    foreach (var stock in _stock)
                    {
                        var _stockDto = new StockDto
                        {
                            StockId = stock.StockId,
                            ItemName = stock.ItemName,
                            StockCategoryID = stock.StockCategoryID,
                            StockCategoryName = stock.StockCategory?.StockCategoryName ?? "",
                            StockUnitTypeID = stock.StockUnitTypeID,
                            StockUnitTypeName = stock.StockUnitType?.StockUnitTypeName ?? "",
                            StockLocationID = stock.StockLocationID,
                            StockLocationName = stock.StockLocation?.StockLocationName ?? "",
                            TotalQTY = stock.TotalQTY,
                            RequiredQTY = stock.RequiredQTY,
                            StockInQTY = stock.StockInQTY,
                            Remark = stock.Remark,
                            RecentStockLogId = stock.RecentStockLogId
                        };
                        StockDto.Add(_stockDto);
                    }
                }
                return StockDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกเมนูจาก Firestore");
                throw;
            }
        }

        public async Task UpdateStockCost(UpdateStockCostDto updateStockCostDto)
        {
            try
            {
                //get cost by id
                // var _cost = await _context.Cost.Where(c => c.CostId == updateStockCostDto.CostDto.CostID).ToListAsync();
                var _cost = await _context.Cost
                                    .Include(c => c.CostCategory)
                                    .Include(c => c.CostStatus)
                                    .FirstOrDefaultAsync(c => c.CostId == updateStockCostDto.CostDto.CostID);
                if (_cost != null)
                {
                    bool IsPurchese = updateStockCostDto.CostDto.IsPurchese;
                    _cost.CostPrice = updateStockCostDto.CostDto.CostPrice;
                    _cost.CostDescription = updateStockCostDto.CostDto.CostDescription;


                    if (IsPurchese)
                    {
                        _cost.IsPurchese = IsPurchese;
                        _cost.PurcheseDate = DateOnly.FromDateTime(System.DateTime.Now);
                        _cost.PurcheseTime = TimeOnly.FromDateTime(System.DateTime.Now);
                        _cost.CostStatusID = 3; //จ่ายเงินแล้ว
                    }
                    else
                    {
                        _cost.CostStatusID = 2; //ยังไม่จ่าย
                    }

                    _cost.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    _cost.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);

                    if (updateStockCostDto.StockDto != null)
                    {
                        foreach (var StockDto in updateStockCostDto.StockDto)
                        {
                            //ค้นหารายการแต่ละ item แล้ว loop add ค่า
                            var _stock = await _context.Stock
                                            .FirstOrDefaultAsync(s => s.StockId == StockDto.StockId);

                            if (_stock != null)
                            {
                                _stock.TotalQTY = StockDto.TotalQTY + StockDto.PurcheseQTY; //จำนวนคงเหลือ
                                _stock.Remark = StockDto.Remark;
                                int StockInQTY = StockDto.StockInQTY - StockDto.PurcheseQTY;//หาจำนวนที่ขาด (ที่ต้องซื้อเพิ่ม)
                                StockInQTY = (StockInQTY < 0) ? 0 : StockInQTY;
                                _stock.StockInQTY = StockInQTY;
                                _stock.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                                _stock.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);

                                if (StockDto.RecentStockLogId != null)
                                {
                                    var _stockLog = await _context.StockLog
                                                         .FirstOrDefaultAsync(s => s.StockLogId == StockDto.RecentStockLogId);

                                    if (_stockLog != null)
                                    {
                                        _stockLog.SupplyID = StockDto.SupplyId;
                                        _stockLog.PurcheseQTY = StockDto.PurcheseQTY;
                                        _stockLog.DipQTY = StockDto.PurcheseQTY - StockDto.StockInQTY;
                                        _stockLog.IsPurchese = IsPurchese;
                                        _stockLog.Price = StockDto.Price;

                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion
        #region  WageCost
        public async Task<List<WorktimeDto>> GetWageCostList()
        {
            try
            {
                // ดึงรายการที่ยังไม่จ่ายขึ้นมาก่อน

                var _Worktime = await _context.Worktime
                                .Include(w => w.Employee)
                                .Where(w => w.IsPurchese == false).ToListAsync();

                var _worktimeDto = new List<WorktimeDto>();
                if (_Worktime != null && _Worktime.Count > 0)
                {

                    foreach (var work in _Worktime)
                    {
                        var WorktimeDto = new WorktimeDto
                        {
                            WorktimeID = work.WorktimeID,
                            WorkDate = work.WorkDate.ToString("yyyy-mm-dd"),
                            TimeClockIn = work.TimeClockIn.ToString(),
                            TimeClockOut = work.TimeClockOut.ToString(),
                            TotalWorktime = work.TotalWorktime,
                            WageCost = work.WageCost,
                            Bonus = work.Bonus,
                            Price = work.Price,
                            IsPurchese = work.IsPurchese,
                            Remark = work.Remark,
                            EmployeeID = work.EmployeeID,
                            EmployeeName = work.Employee.Name
                        };

                        _worktimeDto.Add(WorktimeDto);
                    }
                }
                return _worktimeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกเมนูจาก Firestore");
                throw;
            }
        }

        public async Task UpdateWageCost(WorktimeDto worktime)
        {
            var dateString = worktime.WorkDate;
            DateOnly dateOnly = DateOnly.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var timeString = worktime.TimeClockOut!;
            var timeOnly = TimeOnly.ParseExact(timeString, "HH:mm:ss", CultureInfo.InvariantCulture);
            var _cost = new Cost();

            _cost = new Cost
            {
                CostCategoryID = 2,
                CostPrice = worktime.Price,
                CostDescription = worktime.Remark,
                CostDate = dateOnly,
                CostTime = timeOnly,
                UpdateDate = DateOnly.FromDateTime(System.DateTime.Now),
                UpdateTime = TimeOnly.FromDateTime(System.DateTime.Now)
            };
            bool IsPurchese = worktime.IsPurchese;
            if (IsPurchese)
            {
                _cost.IsPurchese = IsPurchese;
                _cost.PurcheseDate = DateOnly.FromDateTime(System.DateTime.Now);
                _cost.PurcheseTime = TimeOnly.FromDateTime(System.DateTime.Now);
                _cost.CostStatusID = 3; //จ่ายเงินแล้ว
            }
            else
            {
                _cost.CostStatusID = 2; //ยังไม่จ่าย
            }
            await CreateCost(_cost);

            var tmp_Worktime = await _context.Worktime.FirstOrDefaultAsync(w => w.WorktimeID == worktime.WorktimeID);

            //update worktime cost

            if (tmp_Worktime != null)
            {
                if (worktime.TimeClockIn != null)
                {
                    TimeOnly timeOnlyTimeClockIn = TimeOnly.ParseExact(worktime.TimeClockIn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    tmp_Worktime.TimeClockIn = timeOnlyTimeClockIn;
                }
                if (worktime.TimeClockOut != null)
                {
                    TimeOnly timeOnlyTimeClockOut = TimeOnly.ParseExact(worktime.TimeClockOut, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    tmp_Worktime.TimeClockOut = timeOnlyTimeClockOut;
                }
                tmp_Worktime.TotalWorktime = worktime.TotalWorktime;
                tmp_Worktime.WageCost = worktime.WageCost;
                tmp_Worktime.Bonus = worktime.Bonus;
                tmp_Worktime.Price = worktime.Price;
                tmp_Worktime.IsPurchese = worktime.IsPurchese;
                tmp_Worktime.Active = worktime.Active;
                tmp_Worktime.Remark = worktime.Remark;
                tmp_Worktime.UpdateDate = DateOnly.FromDateTime(System.DateTime.Now);
                tmp_Worktime.UpdateTime = TimeOnly.FromDateTime(System.DateTime.Now);
            }
        }
        #endregion
    }
}