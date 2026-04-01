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
        public async Task<List<CostPurchaseType>> GetCostPurchaseTypeList()
        {
            try
            {
                return await _context.CostPurchaseType.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายการประเภทการซื้อ");
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
            cost.UpdateDate = _utilService.GetThailandDate();
            cost.UpdateTime = _utilService.GetThailandTime();
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
                    existingCost.UpdateDate = _utilService.GetThailandDate();
                    existingCost.UpdateTime = _utilService.GetThailandTime();
                    existingCost.UpdateBy = cost.UpdateBy;
                    existingCost.CostPurchaseTypeID = cost.CostPurchaseTypeID;
                    existingCost.CostDate = cost.CostDate;
                    existingCost.CostTime = cost.CostTime;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Cost with ID {CostId} not found for update.", cost.CostId);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการอัพเดทค่าใช้จ่าย" + ex.Message);
                throw;
            }
        }

        #region Stock Cost

        public async Task<List<CostDto>> GetStockCostRequest(CostDto costDto)
        {
            var query = _context.Cost.Include(c => c.CostStatus).Where(c => !c.IsPurchase && c.CostCategoryID == 1 && c.IsActive == true);

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

                var query = _context.Cost.Where(c => !c.IsPurchase && c.CostCategoryID == 1 && c.IsActive == true);

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
                        _cost.PurchaseDate = _utilService.GetThailandDate();
                        _cost.PurchaseTime = _utilService.GetThailandTime();
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
        public async Task UpdateWageCost(List<UpdateWageCostDto> updateWageCostDto)
        {
            // เริ่ม Database Transaction เพื่อป้องกันข้อมูลไม่สมบูรณ์
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ขั้นตอนที่ 1: ตรวจสอบความถูกต้องของข้อมูลเข้า (Input Validation)
                if (updateWageCostDto == null || !updateWageCostDto.Any())
                {
                    throw new ArgumentException("UpdateWageCostDto list is required and cannot be empty");
                }

                var createdCosts = new List<Cost>();
                var updatedWorktimes = new List<Worktime>();

                // ขั้นตอนที่ 2: วนลูปประมวลผลแต่ละรายการ (แต่ละวัน)
                foreach (var item in updateWageCostDto)
                {
                    // ตรวจสอบข้อมูลแต่ละรายการ
                    if (item.EmployeeID <= 0)
                    {
                        throw new ArgumentException($"EmployeeID is required for item: {updateWageCostDto.IndexOf(item)}");
                    }

                    if (item.WageCost <= 0)
                    {
                        throw new ArgumentException($"WageCost must be greater than 0 for EmployeeID: {item.EmployeeID}");
                    }

                    // ขั้นตอนที่ 3: แปลงและตรวจสอบรูปแบบวันที่

                    // ✅ แปลง WorkDate (ต้องมี) - เปลี่ยนจาก StartDate เป็น WorkDate
                    if (string.IsNullOrEmpty(item.WorkDate))
                    {
                        throw new ArgumentException($"WorkDate is required for EmployeeID: {item.EmployeeID}");
                    }

                    if (!DateOnly.TryParseExact(item.WorkDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var workDate))
                    {
                        throw new ArgumentException($"Invalid WorkDate format: {item.WorkDate} for EmployeeID: {item.EmployeeID}");
                    }

                    // ✅ แปลง PurchaseDate (ถ้าไม่มีให้ใช้วันที่ปัจจุบัน)
                    var purchaseDate = item.PurchaseDate ?? _utilService.GetThailandDate().ToString("yyyy-MM-dd");
                    if (!DateOnly.TryParseExact(purchaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var purchaseDateTime))
                    {
                        throw new ArgumentException($"Invalid PurchaseDate format: {purchaseDate} for EmployeeID: {item.EmployeeID}");
                    }

                    // ขั้นตอนที่ 4: สร้างข้อมูลค่าใช้จ่าย (Create Cost Record) สำหรับแต่ละวัน
                    var cost = new Cost
                    {
                        CostCategoryID = 2, // หมวดหมู่ค่าแรง (Wage category)
                        CostPrice = item.WageCost, // จำนวนเงินค่าแรงของวันนี้
                        CostDescription = !string.IsNullOrEmpty(item.Remark)
                            ? item.Remark
                            : $"ค่าแรงพนักงาน ID: {item.EmployeeID} วันที่: {item.WorkDate}",
                        CostDate = workDate, // ✅ ใช้ WorkDate เป็น CostDate
                        CostTime = _utilService.GetThailandTime(), // เวลาที่สร้างรายการ
                        IsPurchase = item.IsPurchase, // สถานะการจ่ายเงิน
                        CostStatusID = item.IsPurchase ? 3 : 2, // 3=จ่ายแล้ว, 2=ยังไม่จ่าย
                        CreateBy = item.CreatedBy ?? 1, // ผู้อัปเดตข้อมูล
                        UpdateDate = _utilService.GetThailandDate(), // วันที่อัปเดต
                        UpdateTime = _utilService.GetThailandTime() // เวลาที่อัปเดต
                    };

                    // ถ้าเป็นการจ่ายเงินจริง ให้บันทึกวันที่และเวลาที่จ่าย
                    if (item.IsPurchase)
                    {
                        cost.PurchaseDate = purchaseDateTime; // ใช้ PurchaseDate ที่ส่งมา
                        cost.PurchaseTime = _utilService.GetThailandTime();
                    }

                    // บันทึก Cost ลงฐานข้อมูลและรับ CostId กลับมา
                    var createdCost = await CreateCostReturnCostID(cost);
                    createdCosts.Add(createdCost);

                    _logger.LogInformation($"💰 Created Cost ID: {createdCost.CostId} for Employee {item.EmployeeID} WorkDate: {item.WorkDate} | Amount: {item.WageCost}");

                    // ขั้นตอนที่ 5: ค้นหาและอัปเดตข้อมูลการทำงาน (Update Worktime Records)

                    // ✅ ค้นหา Worktime สำหรับวันที่และพนักงานที่ระบุ
                    var worktime = await _context.Worktime
                        .Include(w => w.Employee)
                        .FirstOrDefaultAsync(w => w.WorkDate == workDate
                                               && w.EmployeeID == item.EmployeeID
                                               && w.Employee != null
                                               && w.Employee.UserPermistionID != 1); // ไม่รวมเจ้าของ (Owner)

                    if (worktime == null)
                    {
                        _logger.LogWarning($"⚠️ ไม่พบข้อมูลการทำงานของพนักงาน ID {item.EmployeeID} วันที่: {item.WorkDate}");
                        continue; // ข้ามรายการนี้แต่ยังคงสร้าง Cost
                    }

                    // อัปเดตข้อมูลการทำงาน
                    worktime.CostID = createdCost.CostId; // เชื่อมโยงกับ Cost ที่สร้างใหม่
                    worktime.IsPurchase = item.IsPurchase; // อัปเดตสถานะการจ่ายเงิน
                    worktime.Remark = item.Remark ?? ""; // อัปเดตหมายเหตุ
                    worktime.TotalWorktime = item.TotalWorktime; // อัปเดตจำนวนชั่วโมงทำงาน
                    worktime.WageCost = item.WageCost; // อัปเดตค่าแรงของวันนี้
                    worktime.UpdateDate = _utilService.GetThailandDate(); // วันที่อัปเดต
                    worktime.UpdateTime = _utilService.GetThailandTime(); // เวลาที่อัปเดต
                    worktime.UpdateBy = item.CreatedBy ?? 1; // ผู้อัปเดตข้อมูล

                    updatedWorktimes.Add(worktime);

                    _logger.LogInformation($"🔄 Updated worktime record for Employee {item.EmployeeID} WorkDate: {item.WorkDate} with Cost ID: {createdCost.CostId}");
                }

                // ขั้นตอนที่ 6: บันทึกการเปลี่ยนแปลงและยืนยัน Transaction
                await _context.SaveChangesAsync();

                // ขั้นตอนที่ 7: Logging สรุปผลการทำงาน
                _logger.LogInformation($"✅ Batch UpdateWageCost completed:");
                _logger.LogInformation($"📊 Processed {updateWageCostDto.Count} items");
                _logger.LogInformation($"💰 Created {createdCosts.Count} cost records");
                _logger.LogInformation($"🔄 Updated {updatedWorktimes.Count} worktime records");

                // แสดงรายละเอียดแต่ละ Cost ที่สร้าง
                for (int i = 0; i < createdCosts.Count; i++)
                {
                    var cost = createdCosts[i];
                    var originalItem = updateWageCostDto[i];
                    _logger.LogInformation($"💰 Cost ID: {cost.CostId} | Employee: {originalItem.EmployeeID} | WorkDate: {originalItem.WorkDate} | Amount: {cost.CostPrice}");
                }

                // ยืนยัน Transaction (Commit) - ทำให้การเปลี่ยนแปลงถาวร
                await transaction.CommitAsync();

                _logger.LogInformation($"🎯 Batch UpdateWageCost completed successfully");
            }
            catch (Exception ex)
            {
                // ขั้นตอนที่ 8: จัดการข้อผิดพลาด (Error Handling)
                await transaction.RollbackAsync();

                _logger.LogError(ex, "❌ Batch UpdateWageCost failed");
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
                .Include(c => c.CostPurchaseType)
                .Where(c => c.IsPurchase == getCostListDto.IsPurchase && c.IsActive == true);

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
                        CreateDate = c.CreateDate,
                        CreateTime = c.CreateTime,
                        PurchaseDate = c.PurchaseDate,
                        PurchaseTime = c.PurchaseTime,
                        UpdateBy = c.UpdateBy,
                        CostPurchaseTypeID = c.CostPurchaseTypeID,
                        CostPurchaseType = c.CostPurchaseType
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
                    .Where(c => c.IsPurchase == getCostListDto.IsPurchase && c.IsActive == true);

                // กรองหมวดหมู่ค่าใช้จ่าย
                if (getCostListDto.CostCategoryID > 0)
                {
                    query = query.Where(c => c.CostCategoryID == getCostListDto.CostCategoryID);
                }

                       // ✅ เพิ่ม filter DateFrom/DateTo (ถ้ามี)
                if (getCostListDto.StartDate.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue && c.CostDate.Value >= getCostListDto.StartDate.Value);
                }
                if (getCostListDto.EndDate.HasValue)
                {
                    query = query.Where(c => c.CostDate.HasValue && c.CostDate.Value <= getCostListDto.EndDate.Value);
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

                        return new DailyCostReportDto
                        {
                            CostDate = costDate,
                            TotalAmount = (decimal)dateGroup.Sum(x => x.TotalAmount),

                            // ✅ แยกต้นทุนตามหมวดหมู่ (ใช้จาก Cost table แบบเดิม)
                            TotalRawMaterialCost = (decimal)dateGroup
                                .Where(x => x.CategoryId == 1) // วัตถุดิบ
                                .Sum(x => x.TotalAmount),

                            TotalStaffCost = (decimal)dateGroup
                                .Where(x => x.CategoryId == 2) // ค่าจ้างพนักงาน
                                .Sum(x => x.TotalAmount),

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
                    $" | CategoryID: {getCostListDto.CostCategoryID}");

                return dailyReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงรายงานค่าใช้จ่ายรายวัน");
                throw;
            }
        }

        public async Task<List<DailyCostReportDto>> GetCostListbyPurchaseType(DateOnly costDateFrom, DateOnly costDateTo, int costPurchaseTypeId)
        {
            try
            {
                var query = _context.Cost.AsQueryable()
                    .Include(c => c.CostCategory)
                    .Include(c => c.CostStatus)
                    .Where(c => c.IsPurchase == true // ✅ เฉพาะที่จ่ายแล้ว
                        && c.CostDate.HasValue
                        && c.CostDate.Value >= costDateFrom
                        && c.CostDate.Value <= costDateTo
                        && c.IsActive == true);

                // ✅ กรองตามประเภทการซื้อ (ถ้าส่งมา > 0)
                if (costPurchaseTypeId > 0)
                {
                    query = query.Where(c => c.CostPurchaseTypeID == costPurchaseTypeId);
                }

                // ✅ Group by วันที่ และ หมวดหมู่
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

                // ✅ จัดกลุ่มตามวันที่ แล้ว map เป็น DailyCostReportDto
                var dailyReports = groupedCosts
                    .GroupBy(x => x.Date)
                    .Select(dateGroup => new DailyCostReportDto
                    {
                        CostDate = dateGroup.Key,
                        TotalAmount = (decimal)dateGroup.Sum(x => x.TotalAmount),
                        TotalRawMaterialCost = (decimal)dateGroup
                            .Where(x => x.CategoryId == 1)
                            .Sum(x => x.TotalAmount),
                        TotalStaffCost = (decimal)dateGroup
                            .Where(x => x.CategoryId == 2)
                            .Sum(x => x.TotalAmount),
                        TotalOwnerCost = (decimal)dateGroup
                            .Where(x => x.CategoryId == 5)
                            .Sum(x => x.TotalAmount),
                        TotalUtilityCost = (decimal)dateGroup
                            .Where(x => x.CategoryId == 3)
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
                    })
                    .OrderBy(x => x.CostDate) // ✅ เรียงจากวันเก่าไปใหม่
                    .ToList();

                _logger.LogInformation(
                    $"📊 GetCostListbyPurchaseType: Found {dailyReports.Count} daily records" +
                    $" | DateFrom: {costDateFrom}" +
                    $" | DateTo: {costDateTo}" +
                    $" | PurchaseTypeId: {costPurchaseTypeId}");

                return dailyReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ เกิดข้อผิดพลาดในการดึงรายงานค่าใช้จ่ายตามประเภทการซื้อ");
                throw;
            }
        }

    }
}