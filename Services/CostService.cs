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
        public async Task<List<CostCategory>> GetCostCategoryList()
        {
            try
            {
                return await _context.CostCategory.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายการประเภทค่าใช้จ่าย");
                throw;
            }
        }

        public async Task<Cost> CreateCostReturnCostID(Cost cost)
        {
            _context.Cost.Add(cost);
            await _context.SaveChangesAsync();
            return cost;
        }
        public async Task UpdateStockCostDate(DateOnly costDate, int costId, int UpdateBy)
        {
            var cost = await _context.Cost.FirstOrDefaultAsync(c => c.CostId == costId);
            if (cost == null)
            {
                throw new Exception($"ไม่พบค่าใช้จ่ายที่มี ID: {costId}");
            }
            cost.CostDate = costDate;
            cost.UpdateBy = UpdateBy;
            cost.UpdateDate = DateOnly.FromDateTime(System.DateTime.Now);
            cost.UpdateTime = TimeOnly.FromDateTime(System.DateTime.Now);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePurchaseCost(Cost cost)
        {
            try
            {
                var existingCost = await _context.Cost.FirstOrDefaultAsync(c => c.CostId == cost.CostId);
                if (existingCost != null)
                {
                    existingCost.CostCategoryID = cost.CostCategoryID;
                    existingCost.CostPrice = cost.CostPrice;
                    existingCost.CostDescription = cost.CostDescription;
                    existingCost.IsPurchase = cost.IsPurchase;
                    existingCost.PurchaseDate = cost.PurchaseDate;
                    existingCost.PurchaseTime = cost.PurchaseTime;
                    existingCost.CostStatusID = cost.CostStatusID;
                    existingCost.UpdateDate = DateOnly.FromDateTime(System.DateTime.Now);
                    existingCost.UpdateTime = TimeOnly.FromDateTime(System.DateTime.Now);
                    existingCost.UpdateBy = cost.UpdateBy;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Cost with ID {CostId} not found for update.", cost.CostId);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการอัพเดทค่าใช้จ่าย");
                throw;
            }
        }

        #region Stock Cost

        public async Task<List<CostDto>> GetStockCostRequest(CostDto costDto)
        {
            var query = _context.Cost.Include(c => c.CostStatus).Where(c => !c.IsPurchase && c.CostCategoryID == 1);

            if (costDto is { CostDate: not null and var date } && date != default)
            {
                query = query.Where(c => c.CostDate == date);
            }
            //สำหรับกรองจ่ายเงินหรือยังไม่จ่ายเงิน ถ้าไม่ต้องการกรองให้ส่งค่า null
            if (costDto.CostStatusID != null)
            {
                query = query.Where(c => c.CostStatusID == costDto.CostStatusID);
            }
            var _cost = await query.OrderByDescending(c => c.CostDate).ThenByDescending(c => c.CostId).ToListAsync();

            var CostDto = new List<CostDto>();
            if (_cost != null && _cost.Count > 0)
            {
                foreach (var cost in _cost)
                {
                    CostDto.Add(new CostDto
                    {
                        CostID = cost.CostId,
                        CostCategoryID = cost.CostCategoryID,
                        CostPrice = cost.CostPrice,
                        CostDescription = cost.CostDescription ?? "",
                        CostDate = cost.CostDate,
                        CostTime = cost.CostTime,
                        CostStatusID = cost.CostStatusID,
                        CostStatus = cost.CostStatus
                    });
                }
            }
            return CostDto;
        }
        public async Task<List<StockDto>> GetStockCostList(CostDto costDto) //http://localhost:5036/api/stock/GetStockCostList
        {
            try
            {
                //หาก่อนว่าในวันนี้มีรายการที่ต้องสั่งซื้อมั้ย
                //var _cost = await _context.Cost.Where(c => c.CostDate == costDto.CostDate && !c.IsPurchase).ToListAsync();

                var query = _context.Cost.Where(c => !c.IsPurchase && c.CostCategoryID == 1);

                if (costDto is { CostDate: not null and var date } && date != default)
                {
                    query = query.Where(c => c.CostDate == date);
                }
                //สำหรับกรองจ่ายเงินหรือยังไม่จ่ายเงิน ถ้าไม่ต้องการกรองให้ส่งค่า null
                if (costDto.CostStatusID != null)
                {
                    query = query.Where(c => c.CostStatusID == costDto.CostStatusID);
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
                                    .FirstOrDefaultAsync(c => c.CostId == updateStockCostDto.CostID);
                if (_cost != null)
                {
                    bool IsPurchase = updateStockCostDto.IsPurchase;
                    _cost.CostPrice = updateStockCostDto.CostPrice;
                    // _cost.CostDescription = updateStockCostDto.CostDescription;


                    if (IsPurchase)
                    {
                        _cost.IsPurchase = IsPurchase;
                        _cost.PurchaseDate = DateOnly.FromDateTime(System.DateTime.Now);
                        _cost.PurchaseTime = TimeOnly.FromDateTime(System.DateTime.Now);
                        _cost.CostStatusID = 3; //จ่ายเงินแล้ว
                    }
                    else
                    {
                        _cost.CostStatusID = 2; //ยังไม่จ่าย
                    }
                    _cost.UpdateBy = updateStockCostDto.UpdateBy;
                    _cost.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    _cost.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);

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
                                .Where(w => w.IsPurchase == false).ToListAsync();

                var _worktimeDto = new List<WorktimeDto>();
                if (_Worktime != null && _Worktime.Count > 0)
                {

                    foreach (var work in _Worktime)
                    {
                        var WorktimeDto = new WorktimeDto
                        {
                            WorktimeID = work.WorktimeID,
                            WorkDate = work.WorkDate.ToString("yyyy-MM-dd"),
                            TimeClockIn = work.TimeClockIn.ToString(),
                            TimeClockOut = work.TimeClockOut.ToString(),
                            TotalWorktime = work.TotalWorktime,
                            WageCost = work.WageCost,
                            Bonus = work.Bonus,
                            Price = work.Price,
                            IsPurchase = work.IsPurchase,
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

        /// <summary>
        /// อัปเดตค่าแรงพนักงาน โดยสร้างรายการค่าใช้จ่าย (Cost) และเชื่อมโยงกับข้อมูลการทำงาน (Worktime)
        /// </summary>
        /// <param name="worktimeDto">ข้อมูลค่าแรงที่ต้องการอัปเดต</param>
        public async Task UpdateWageCost(WorktimeDto worktimeDto)
        {
            // เริ่ม Database Transaction เพื่อป้องกันข้อมูลไม่สมบูรณ์
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // ขั้นตอนที่ 1: ตรวจสอบความถูกต้องของข้อมูลเข้า (Input Validation)
                
                // ตรวจสอบว่ามี EmployeeID และมีค่ามากกว่า 0
                if (worktimeDto.EmployeeID <= 0)
                {
                    throw new ArgumentException("EmployeeID is required");
                }

                // ตรวจสอบว่าค่าแรงมีค่ามากกว่า 0
                if (worktimeDto.WageCost <= 0)
                {
                    throw new ArgumentException("WageCost must be greater than 0");
                }

                // ขั้นตอนที่ 2: แปลงและตรวจสอบรูปแบบวันที่ (Date Parsing & Validation)
                
                // กำหนดวันที่จ่ายเงิน: ใช้ PurchaseDate ที่ส่งมา หรือวันที่ปัจจุบันถ้าไม่มี
                var purchaseDate = worktimeDto.PurchaseDate ?? DateTime.Now.ToString("yyyy-MM-dd");
                
                // แปลงและตรวจสอบรูปแบบวันที่จ่ายเงิน (ต้องเป็น yyyy-MM-dd)
                if (!DateOnly.TryParseExact(purchaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var costDate))
                {
                    throw new ArgumentException($"Invalid PurchaseDate format: {purchaseDate}");
                }

                DateOnly startDate, endDate;
                
                // แปลงและตรวจสอบวันที่เริ่มต้น (StartDate)
                if (!string.IsNullOrEmpty(worktimeDto.StartDate))
                {
                    // ถ้ามี StartDate ให้แปลงจาก string เป็น DateOnly
                    if (!DateOnly.TryParseExact(worktimeDto.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                    {
                        throw new ArgumentException($"Invalid StartDate format: {worktimeDto.StartDate}");
                    }
                }
                else
                {
                    // ถ้าไม่มี StartDate ให้ใช้วันที่จ่ายเงินเป็นค่าเริ่มต้น
                    startDate = costDate;
                }

                // แปลงและตรวจสอบวันที่สิ้นสุด (EndDate)
                if (!string.IsNullOrEmpty(worktimeDto.EndDate))
                {
                    // ถ้ามี EndDate ให้แปลงจาก string เป็น DateOnly
                    if (!DateOnly.TryParseExact(worktimeDto.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    {
                        throw new ArgumentException($"Invalid EndDate format: {worktimeDto.EndDate}");
                    }
                }
                else
                {
                    // ถ้าไม่มี EndDate ให้ใช้วันที่จ่ายเงินเป็นค่าเริ่มต้น
                    endDate = costDate;
                }

                // ตรวจสอบความสมเหตุสมผลของช่วงวันที่
                if (startDate > endDate)
                {
                    throw new ArgumentException("StartDate cannot be greater than EndDate");
                }

                // ขั้นตอนที่ 3: สร้างข้อมูลค่าใช้จ่าย (Create Cost Record)
                
                var cost = new Cost
                {
                    CostCategoryID = 2, // หมวดหมู่ค่าแรง (Wage category)
                    CostPrice = worktimeDto.WageCost, // จำนวนเงินค่าแรง
                    CostDescription = worktimeDto.Remark ?? $"ค่าแรงพนักงาน ID: {worktimeDto.EmployeeID}", // รายละเอียด
                    CostDate = costDate, // วันที่เกิดค่าใช้จ่าย
                    CostTime = TimeOnly.FromDateTime(DateTime.Now), // เวลาที่เกิดค่าใช้จ่าย
                    IsPurchase = worktimeDto.IsPurchase, // สถานะการจ่ายเงิน
                    CostStatusID = worktimeDto.IsPurchase ? 3 : 2, // 3=จ่ายแล้ว, 2=ยังไม่จ่าย
                    UpdateBy = worktimeDto.CreatedBy ?? 1, // ผู้อัปเดตข้อมูล
                    UpdateDate = DateOnly.FromDateTime(DateTime.Now), // วันที่อัปเดต
                    UpdateTime = TimeOnly.FromDateTime(DateTime.Now) // เวลาที่อัปเดต
                };

                // ถ้าเป็นการจ่ายเงินจริง ให้บันทึกวันที่และเวลาที่จ่าย
                if (worktimeDto.IsPurchase)
                {
                    cost.PurchaseDate = DateOnly.FromDateTime(DateTime.Now);
                    cost.PurchaseTime = TimeOnly.FromDateTime(DateTime.Now);
                }

                // บันทึก Cost ลงฐานข้อมูลและรับ CostId กลับมา
                var createdCost = await CreateCostReturnCostID(cost);
                _logger.LogInformation($"💰 Created Cost ID: {createdCost.CostId} for Employee {worktimeDto.EmployeeID}");

                // ขั้นตอนที่ 4: ค้นหาและอัปเดตข้อมูลการทำงาน (Update Worktime Records)
                
                // ค้นหาข้อมูลการทำงานของพนักงานในช่วงวันที่ที่กำหนด
                var worktimes = await _context.Worktime
                    .Include(w => w.Employee) // รวมข้อมูลพนักงาน
                    .Where(w => w.WorkDate >= startDate // วันที่ทำงาน >= วันเริ่มต้น
                             && w.WorkDate <= endDate // วันที่ทำงาน <= วันสิ้นสุด
                             && w.Employee != null // ต้องมีข้อมูลพนักงาน
                             && w.Employee.UserPermistionID != 1 // ไม่รวมเจ้าของ (Owner)
                             && w.EmployeeID == worktimeDto.EmployeeID) // พนักงานคนที่ระบุ
                    .ToListAsync();

                // ตรวจสอบว่าพบข้อมูลการทำงานหรือไม่
                if (!worktimes.Any())
                {
                    throw new InvalidOperationException($"ไม่พบข้อมูลการทำงานของพนักงาน ID {worktimeDto.EmployeeID} ในช่วง {startDate} ถึง {endDate}");
                }

                // อัปเดตข้อมูลการทำงานแต่ละรายการ
                var totalUpdated = 0;
                foreach (var worktime in worktimes)
                {
                    worktime.CostID = createdCost.CostId; // เชื่อมโยงกับ Cost ที่สร้างใหม่
                    worktime.IsPurchase = worktimeDto.IsPurchase; // อัปเดตสถานะการจ่ายเงิน
                    worktime.Remark = worktimeDto.Remark ?? ""; // อัปเดตหมายเหตุ
                    worktime.UpdateDate = DateOnly.FromDateTime(DateTime.Now); // วันที่อัปเดต
                    worktime.UpdateTime = TimeOnly.FromDateTime(DateTime.Now); // เวลาที่อัปเดต
                    totalUpdated++;
                }

                // ขั้นตอนที่ 5: บันทึกการเปลี่ยนแปลงและยืนยัน Transaction
                
                // บันทึกการเปลี่ยนแปลงทั้งหมดลงฐานข้อมูล
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"✅ Updated {totalUpdated} worktime records with Cost ID: {createdCost.CostId}");

                // ยืนยัน Transaction (Commit) - ทำให้การเปลี่ยนแปลงถาวร
                await transaction.CommitAsync();
                
                _logger.LogInformation($"🎯 UpdateWageCost completed successfully for Employee {worktimeDto.EmployeeID}");
            }
            catch (Exception ex)
            {
                // ขั้นตอนที่ 6: จัดการข้อผิดพลาด (Error Handling)
                
                // ยกเลิก Transaction (Rollback) - เพื่อคืนข้อมูลกลับสู่สถานะเดิม
                await transaction.RollbackAsync();
                
                // บันทึก Error Log
                _logger.LogError(ex, "❌ UpdateWageCost failed for Employee {EmployeeID}", worktimeDto.EmployeeID);
                
                // ส่ง Exception ต่อไปยัง caller
                throw;
            }
        }

        public async Task<List<CostDto>> GetAllCostList(CostDto costDto)
        {
            try
            {
                var query = _context.Cost.AsQueryable()
                .Include(c => c.CostCategory)
                .Include(c => c.CostStatus)
                .Where(c => c.IsPurchase == costDto.IsPurchase);

                //กรองหมวดหมู่ค่าใช้จ่าย
                if (costDto.CostCategoryID > 0)
                {
                    query = query.Where(c => c.CostCategoryID == costDto.CostCategoryID);
                }
                //กรองวันที่ค่าใช้จ่าย
                if (costDto.CostDate.HasValue)
                {
                    query = query.Where(c => c.CostDate == costDto.CostDate.Value);
                }

                var costs = await query.OrderByDescending(c => c.CostDate).ThenByDescending(c => c.CostTime).ToListAsync();

                return costs.Select(c => new CostDto
                {
                    CostID = c.CostId,
                    CostCategoryID = c.CostCategoryID,
                    costCategory = c.CostCategory,
                    CostPrice = c.CostPrice,
                    CostDescription = c.CostDescription ?? string.Empty,
                    CostDate = c.CostDate,
                    CostTime = c.CostTime,
                    UpdateDate = c.UpdateDate,
                    UpdateTime = c.UpdateTime,
                    IsPurchase = c.IsPurchase,
                    CostStatusID = c.CostStatusID,
                    CostStatus = c.CostStatus,
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายการค่าใช้จ่าย");
                throw;
            }
        }
        #endregion

    }
}