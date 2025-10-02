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
        private readonly IUtilService _utilService;
        public CostService(ChickkoContext context, ILogger<StockService> logger, IUtilService utilService)
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
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
                    if (CheckHaveItemInStockCost(cost.CostId))
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
                    _cost.UpdateDate = _utilService.GetThailandDate();
                    _cost.UpdateTime = _utilService.GetThailandTime();

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
        public async Task UpdateWageCost(UpdateWageCostDto updateWageCostDto)
        {
            // เริ่ม Database Transaction เพื่อป้องกันข้อมูลไม่สมบูรณ์
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ขั้นตอนที่ 1: ตรวจสอบความถูกต้องของข้อมูลเข้า (Input Validation)

                // ตรวจสอบว่ามี EmployeeID และมีค่ามากกว่า 0
                if (updateWageCostDto.EmployeeID <= 0)
                {
                    throw new ArgumentException("EmployeeID is required");
                }

                // ตรวจสอบว่าค่าแรงมีค่ามากกว่า 0
                if (updateWageCostDto.WageCost <= 0)
                {
                    throw new ArgumentException("WageCost must be greater than 0");
                }

                // ✅ ตรวจสอบ WorkDatePurchase array
                if (updateWageCostDto.WorkDatePurchase == null || !updateWageCostDto.WorkDatePurchase.Any())
                {
                    throw new ArgumentException("WorkDatePurchase is required and cannot be empty");
                }

                // ขั้นตอนที่ 2: แปลงและตรวจสอบรูปแบบวันที่ (Date Parsing & Validation)

                // กำหนดวันที่จ่ายเงิน: ใช้ PurchaseDate ที่ส่งมา หรือวันที่ปัจจุบันถ้าไม่มี
                var purchaseDate = updateWageCostDto.PurchaseDate ?? DateTime.Now.ToString("yyyy-MM-dd");

                // แปลงและตรวจสอบรูปแบบวันที่จ่ายเงิน (ต้องเป็น yyyy-MM-dd)
                if (!DateOnly.TryParseExact(purchaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var costDate))
                {
                    throw new ArgumentException($"Invalid PurchaseDate format: {purchaseDate}");
                }

                // ✅ แปลง WorkDatePurchase strings เป็น DateOnly array
                var workDatesToUpdate = new List<DateOnly>();
                foreach (var dateStr in updateWageCostDto.WorkDatePurchase)
                {
                    if (DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var workDate))
                    {
                        workDatesToUpdate.Add(workDate);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid WorkDate format: {dateStr}");
                    }
                }

                // ขั้นตอนที่ 3: สร้างข้อมูลค่าใช้จ่าย (Create Cost Record)

                var cost = new Cost
                {
                    CostCategoryID = 2, // หมวดหมู่ค่าแรง (Wage category)
                    CostPrice = updateWageCostDto.WageCost, // จำนวนเงินค่าแรง
                    CostDescription = updateWageCostDto.Remark ?? $"ค่าแรงพนักงาน ID: {updateWageCostDto.EmployeeID}", // รายละเอียด
                    CostDate = costDate, // วันที่เกิดค่าใช้จ่าย
                    CostTime = TimeOnly.FromDateTime(DateTime.Now), // เวลาที่เกิดค่าใช้จ่าย
                    IsPurchase = updateWageCostDto.IsPurchase, // สถานะการจ่ายเงิน
                    CostStatusID = updateWageCostDto.IsPurchase ? 3 : 2, // 3=จ่ายแล้ว, 2=ยังไม่จ่าย
                    UpdateBy = updateWageCostDto.CreatedBy ?? 1, // ผู้อัปเดตข้อมูล
                    UpdateDate = DateOnly.FromDateTime(DateTime.Now), // วันที่อัปเดต
                    UpdateTime = TimeOnly.FromDateTime(DateTime.Now) // เวลาที่อัปเดต
                };

                // ถ้าเป็นการจ่ายเงินจริง ให้บันทึกวันที่และเวลาที่จ่าย
                if (updateWageCostDto.IsPurchase)
                {
                    cost.PurchaseDate = _utilService.GetThailandDate();
                    cost.PurchaseTime = _utilService.GetThailandTime();
                }

                // บันทึก Cost ลงฐานข้อมูลและรับ CostId กลับมา
                var createdCost = await CreateCostReturnCostID(cost);
                _logger.LogInformation($"💰 Created Cost ID: {createdCost.CostId} for Employee {updateWageCostDto.EmployeeID}");

                // ขั้นตอนที่ 4: ค้นหาและอัปเดตข้อมูลการทำงาน (Update Worktime Records)

                // ✅ ค้นหาข้อมูลการทำงานเฉพาะวันที่ที่ระบุใน WorkDatePurchase
                var worktimes = await _context.Worktime
                    .Include(w => w.Employee) // รวมข้อมูลพนักงาน
                    .Where(w => workDatesToUpdate.Contains(w.WorkDate) // ✅ เฉพาะวันที่ที่ระบุ
                             && w.Employee != null // ต้องมีข้อมูลพนักงาน
                             && w.Employee.UserPermistionID != 1 // ไม่รวมเจ้าของ (Owner)
                             && w.EmployeeID == updateWageCostDto.EmployeeID) // พนักงานคนที่ระบุ
                    .ToListAsync();

                // ตรวจสอบว่าพบข้อมูลการทำงานหรือไม่
                if (!worktimes.Any())
                {
                    var dateList = string.Join(", ", workDatesToUpdate.Select(d => d.ToString("yyyy-MM-dd")));
                    throw new InvalidOperationException($"ไม่พบข้อมูลการทำงานของพนักงาน ID {updateWageCostDto.EmployeeID} ในวันที่: {dateList}");
                }

                // ✅ ตรวจสอบว่าพบครบทุกวันที่ที่ระบุหรือไม่
                var foundWorkDates = worktimes.Select(w => w.WorkDate).ToHashSet();
                var missingDates = workDatesToUpdate.Where(d => !foundWorkDates.Contains(d)).ToList();
                
                if (missingDates.Any())
                {
                    var missingDateList = string.Join(", ", missingDates.Select(d => d.ToString("yyyy-MM-dd")));
                    _logger.LogWarning($"⚠️ ไม่พบข้อมูลการทำงานในวันที่: {missingDateList} สำหรับพนักงาน ID {updateWageCostDto.EmployeeID}");
                }

                // อัปเดตข้อมูลการทำงานแต่ละรายการ
                var totalUpdated = 0;
                var updatedDates = new List<string>();
                
                foreach (var worktime in worktimes)
                {
                    worktime.CostID = createdCost.CostId; // เชื่อมโยงกับ Cost ที่สร้างใหม่
                    worktime.IsPurchase = updateWageCostDto.IsPurchase; // อัปเดตสถานะการจ่ายเงิน
                    worktime.Remark = updateWageCostDto.Remark ?? ""; // อัปเดตหมายเหตุ
                    worktime.UpdateDate = _utilService.GetThailandDate(); // วันที่อัปเดต
                    worktime.UpdateTime = _utilService.GetThailandTime(); // เวลาที่อัปเดต
                    worktime.UpdateBy = updateWageCostDto.CreatedBy ?? 1; // ผู้อัปเดตข้อมูล
                    updatedDates.Add(worktime.WorkDate.ToString("yyyy-MM-dd"));
                    totalUpdated++;
                }

                // ขั้นตอนที่ 5: บันทึกการเปลี่ยนแปลงและยืนยัน Transaction

                // บันทึกการเปลี่ยนแปลงทั้งหมดลงฐานข้อมูล
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Updated {totalUpdated} worktime records with Cost ID: {createdCost.CostId}");
                _logger.LogInformation($"📅 Updated work dates: {string.Join(", ", updatedDates)}");

                // ยืนยัน Transaction (Commit) - ทำให้การเปลี่ยนแปลงถาวร
                await transaction.CommitAsync();

                _logger.LogInformation($"🎯 UpdateWageCost completed successfully for Employee {updateWageCostDto.EmployeeID}");
            }
            catch (Exception ex)
            {
                // ขั้นตอนที่ 6: จัดการข้อผิดพลาด (Error Handling)

                // ยกเลิก Transaction (Rollback) - เพื่อคืนข้อมูลกลับสู่สถานะเดิม
                await transaction.RollbackAsync();

                // บันทึก Error Log
                _logger.LogError(ex, "❌ UpdateWageCost failed for Employee {EmployeeID}", updateWageCostDto.EmployeeID);

                // ส่ง Exception ต่อไปยัง caller
                throw;
            }
        }

        public async Task<List<CostDto>> GetAllCostList(GetCostListDto getCostListDto)
        {
            try
            {
                var query = _context.Cost.AsQueryable()
                .Include(c => c.CostCategory)
                .Include(c => c.CostStatus)
                .Where(c => c.IsPurchase == getCostListDto.IsPurchase);

                //กรองหมวดหมู่ค่าใช้จ่าย
                if (getCostListDto.CostCategoryID > 0)
                {
                    query = query.Where(c => c.CostCategoryID == getCostListDto.CostCategoryID);
                }


                // Filter by Year and Month if provided
                if (getCostListDto.Year.HasValue && getCostListDto.Month.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue &&
                        c.CostDate.Value.Year == getCostListDto.Year.Value &&
                        c.CostDate.Value.Month == getCostListDto.Month.Value);
                }
                else if (getCostListDto.Year.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue &&
                        c.CostDate.Value.Year == getCostListDto.Year.Value);
                }
                else if (getCostListDto.Month.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue &&
                        c.CostDate.Value.Month == getCostListDto.Month.Value);
                }

                var costs = await query.OrderByDescending(c => c.CostDate).ThenByDescending(c => c.CostTime).ToListAsync();

                var result = costs.Select(c =>
                {
                    var dto = new CostDto
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
                    };

                    // ✅ เงื่อนไขพิเศษสำหรับ CostCategoryID = 1 // Stock Cost
                    if (c.CostCategoryID == 1)
                    {
                        dto.IsStockIn = CheckHaveItemInStockCost(c.CostId);
                    }
                    else
                    {
                        dto.IsStockIn = false; // กรณีไม่ใช่ Stock Cost ให้ตั้งค่าเป็น false
                    }

                    return dto;
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายการค่าใช้จ่าย");
                throw;
            }
        }

        public bool CheckHaveItemInStockCost(int costId)
        {
            try
            {
                // ตรวจสอบว่ามีการเชื่อมโยงกับ StockLog หรือไม่
                var linkedStockLogs = _context.StockLog.Any(w => w.CostId == costId);

                if (linkedStockLogs)
                {
                    return true; // มีการเชื่อมโยง
                }
                else
                {
                    return false; // ไม่มีการเชื่อมโยง
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการตรวจสอบการเชื่อมโยงค่าใช้จ่ายกับข้อมูลการทำงานที่มี ID: {CostId}", costId);
                throw;
            }
        }
        #endregion

        public async Task<string> DeleteCost(int costId)
        {
            try
            {
                var cost = await _context.Cost.FirstOrDefaultAsync(c => c.CostId == costId);
                if (cost == null)
                {
                    return $"ไม่พบค่าใช้จ่ายที่มี ID: {costId}";
                }

                // ตรวจสอบว่ามีการเชื่อมโยงกับ Worktime หรือไม่
                var linkedWorktimes = await _context.Worktime.AnyAsync(w => w.CostID == costId);
                if (linkedWorktimes)
                {
                    return $"ไม่สามารถลบค่าใช้จ่ายที่มี ID: {costId} เนื่องจากมีการเชื่อมโยงกับข้อมูลการทำงาน";
                }

                _context.Cost.Remove(cost);
                await _context.SaveChangesAsync();
                return $"ลบค่าใช้จ่ายที่มี ID: {costId} เรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบค่าใช้จ่ายที่มี ID: {CostId}", costId);
                throw;
            }
        }
        public async Task<List<DailyCostReportDto>> GetCostListReport(GetCostListDto getCostListDto)
        {
            try
            {
                var query = _context.Cost.AsQueryable()
                    .Include(c => c.CostCategory)
                    .Include(c => c.CostStatus)
                    .Where(c => c.IsPurchase == getCostListDto.IsPurchase);

                // ✅ ดึงข้อมูล Worktime สำหรับคำนวณค่าแรงพนักงาน
                var worktimeQuery = _context.Worktime
                    .Where(w => w.IsPurchase == getCostListDto.IsPurchase);

                // กรองหมวดหมู่ค่าใช้จ่าย
                if (getCostListDto.CostCategoryID > 0)
                {
                    query = query.Where(c => c.CostCategoryID == getCostListDto.CostCategoryID);
                }

                // Filter by Year and Month if provided
                if (getCostListDto.Year.HasValue && getCostListDto.Month.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue &&
                        c.CostDate.Value.Year == getCostListDto.Year.Value &&
                        c.CostDate.Value.Month == getCostListDto.Month.Value);

                    worktimeQuery = worktimeQuery.Where(w =>
                    w.WorkDate.Year == getCostListDto.Year.Value &&
                    w.WorkDate.Month == getCostListDto.Month.Value);
                }
                else if (getCostListDto.Year.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue &&
                        c.CostDate.Value.Year == getCostListDto.Year.Value);

                    worktimeQuery = worktimeQuery.Where(w =>
                   w.WorkDate.Year == getCostListDto.Year.Value);
                }
                else if (getCostListDto.Month.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue &&
                        c.CostDate.Value.Month == getCostListDto.Month.Value);

                    worktimeQuery = worktimeQuery.Where(w =>
                    w.WorkDate.Month == getCostListDto.Month.Value);
                }

                // ✅ ดึงข้อมูล Worktime และจัดกลุ่มตามวันที่
                var dailyWorktimeCosts = await worktimeQuery
                    .GroupBy(w => w.WorkDate)
                    .Select(g => new
                    {
                        WorkDate = g.Key,
                        TotalWageCost = g.Sum(w => w.WageCost)
                    })
                    .ToListAsync();

                // ✅ รวมข้อมูลตามวันที่และหมวดหมู่
                var groupedCosts = await query
                    .GroupBy(c => new
                    {
                        Date = c.CostDate,
                        CategoryId = c.CostCategoryID,
                        CategoryName = c.CostCategory!.CostCategoryName
                    })
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        TotalAmount = g.Sum(x => x.CostPrice),
                        Count = g.Count()
                    })
                    .ToListAsync();

                // ✅ จัดกลุ่มตามวันที่เพื่อสร้าง DailyCostReportDto พร้อมแยกต้นทุนตามหมวดหมู่
                var dailyReports = groupedCosts
                    .GroupBy(x => x.Date)
                    .Select(dateGroup =>
                    {
                        var costDate = dateGroup.Key;

                        // ✅ คำนวณค่าแรงพนักงาน - ใช้ Worktime ก่อน แล้วค่อย fallback ไปใช้ Cost
                        decimal totalStaffCost = 0;

                        // ตรวจสอบว่ามีข้อมูล Worktime สำหรับวันนี้หรือไม่
                        var worktimeCost = dailyWorktimeCosts
                            .FirstOrDefault(w => w.WorkDate == costDate);

                        if (worktimeCost != null && worktimeCost.TotalWageCost > 0)
                        {
                            // ✅ ใช้ค่าจาก Worktime table
                            totalStaffCost = (decimal)worktimeCost.TotalWageCost;
                        }
                        else if (costDate < new DateOnly(2025, 9, 17))
                        {
                            // ✅ Fallback: ใช้ค่าจาก Cost table (วิธีเดิม) - เฉพาะข้อมูลก่อน 2025-09-17
                            totalStaffCost = (decimal)dateGroup
                                .Where(x => x.CategoryId == 2) // ค่าจ้างพนักงาน
                                .Sum(x => x.TotalAmount);
                        }
                        else
                        {
                            // ✅ หลังวันที่ 2025-09-17 ถ้าไม่มีข้อมูลใน Worktime ให้เป็น 0
                            totalStaffCost = 0;
                        }

                        return new DailyCostReportDto
                        {
                            CostDate = costDate,
                            TotalAmount = (decimal)dateGroup.Sum(x => x.TotalAmount) + totalStaffCost, // ✅ รวมค่าแรงใหม่

                            // ✅ แยกต้นทุนตามหมวดหมู่
                            TotalRawMaterialCost = (decimal)dateGroup
                                .Where(x => x.CategoryId == 1) // วัตถุดิบ
                                .Sum(x => x.TotalAmount),

                            TotalStaffCost = totalStaffCost, // ✅ ใช้ค่าที่คำนวณใหม่

                            TotalOwnerCost = (decimal)dateGroup
                                .Where(x => x.CategoryId == 5) // ค่าใช้จ่ายเจ้าของ
                                .Sum(x => x.TotalAmount),

                            TotalUtilityCost = (decimal)dateGroup
                                .Where(x => x.CategoryId == 3) // ค่าสาธารณูปโภค
                                .Sum(x => x.TotalAmount),

                            TotalOtherCost = (decimal)dateGroup
                                .Where(x => x.CategoryId != 1 && x.CategoryId != 2 && x.CategoryId != 3 && x.CategoryId != 5)
                                .Sum(x => x.TotalAmount),

                            CategoryDetails = dateGroup.Select(cat => new CostCategoryDetailDto
                            {
                                CostCategoryID = cat.CategoryId,
                                CategoryName = cat.CategoryName ?? "ไม่ระบุ",
                                TotalAmount = (decimal)cat.TotalAmount,
                                Count = cat.Count
                            }).ToList()
                        };
                    })
                    .OrderByDescending(x => x.CostDate)
                    .ToList();

                // ✅ เพิ่ม logging สำหรับ debug
                _logger.LogInformation($"📊 GetCostListReport: Found {dailyReports.Count} daily records" +
                    $" | Year: {getCostListDto.Year}" +
                    $" | Month: {getCostListDto.Month}" +
                    $" | CategoryID: {getCostListDto.CostCategoryID}" +
                    $" | Worktime records: {dailyWorktimeCosts.Count}");

                return dailyReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายงานค่าใช้จ่ายรายวัน");
                throw;
            }
        }

    }
}