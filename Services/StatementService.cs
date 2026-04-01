using System.Linq;
using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services
{

    public class StatementService : IStatementService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<StatementService> _logger;
        private readonly IUtilService _utilService;
        private readonly IOrdersService _ordersService;
        private readonly ICostService _costService; // เพิ่มฟิลด์สำหรับ ICostService

        public StatementService(
            ChickkoContext context,
            ILogger<StatementService> logger,
            IUtilService utilService,
            IOrdersService ordersService, // เพิ่ม DI IOrdersService
            ICostService costService // เพิ่ม DI ICostService
        )
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
            _ordersService = ordersService; // กำหนดให้ field
            _costService = costService; // กำหนดให้ field
        }


        /// <summary>
        /// บันทึกรายการรายรับใหม่ลงในฐานข้อมูล
        /// </summary>
        /// <param name="income">ข้อมูลรายรับที่ได้รับจาก request</param>
        public async Task InsertIncomeAsync(IncomeStatementDto income)
        {  
            try
            {
                // สร้าง object Income ใหม่และ map ข้อมูลจาก DTO
                Income InsertIncome = new Income
                {
                    IncomeDate = income.IncomeDate,           // วันที่ของรายรับ
                    IncomeValue = income.IncomeValue,         // จำนวนเงินรายรับ
                    IncomeTime = income.IncomeTime,           // เวลาของรายรับ
                    IncomeTypeId = income.IncomeTypeId,       // ประเภทของรายรับ (FK)
                    UserId = income.UserId,                   // ผู้บันทึกรายการ (FK)
                    IncomeDescription = income.IncomeDescription, // คำอธิบายเพิ่มเติม
                    UpdateDate = _utilService.GetThailandDate(),  // วันที่อัปเดตล่าสุด (เวลาไทย)
                    UpdateTime = _utilService.GetThailandTime()   // เวลาอัปเดตล่าสุด (เวลาไทย)
                };

                // เพิ่ม record ลงใน DbContext (ยังไม่บันทึกลง DB)
                _context.Incomes.Add(InsertIncome);

                // บันทึกการเปลี่ยนแปลงทั้งหมดลงฐานข้อมูลจริง
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // บันทึก log error และโยน exception ต่อให้ caller จัดการ
                _logger.LogError(ex, "Error inserting income");
                throw;
            }
            
        }

        public async Task InsertStatementAsync(StatementDto statementDto)
        {
            try
            {
                Statement InsertStatement = new Statement
                {
                    StatementDate = statementDto.StatementDate,
                    StatementValue = statementDto.StatementValue,
                    StatementTime = statementDto.StatementTime,
                    StatementDescription = statementDto.StatementDescription,
                    UserId = statementDto.UserId,
                    UpdateDate = _utilService.GetThailandDate(),
                    UpdateTime = _utilService.GetThailandTime()
                };
                _context.Statements.Add(InsertStatement);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting statement");
                throw;
            }
        }
        public async Task<List<IncomeType>> GetIncomeTypesAsync()
        {
            try
            {
                var incomeTypes = await _context.IncomeTypes.Where(it => it.Active).ToListAsync();
                return incomeTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving income types");
                throw;
            }
        }

        public async Task<List<IncomeStatementDto>> GetIncomeAsync(DateOnly? dateFrom, DateOnly? dateTo, int? incomeTypeId = null)
        {
            try
            {
                // ถ้าไม่ได้ส่งช่วงวันที่มา ให้ default เป็นเดือนปัจจุบัน
                var now = DateTime.Now;
                var firstDay = new DateOnly(now.Year, now.Month, 1);
                var lastDay = new DateOnly(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                dateFrom ??= firstDay;
                dateTo ??= lastDay;

                var query = _context.Incomes
                    .Where(i => i.IncomeDate >= dateFrom && i.IncomeDate <= dateTo);

                // ✅ กรอง IncomeTypeId ถ้าส่งมา
                if (incomeTypeId.HasValue && incomeTypeId.Value > 0)
                {
                    query = query.Where(i => i.IncomeTypeId == incomeTypeId.Value);
                }

                var incomes = await query
                    .OrderByDescending(i => i.IncomeDate)
                    .ThenByDescending(i => i.IncomeTime)
                    .Join(_context.IncomeTypes,
                        income => income.IncomeTypeId,
                        type => type.IncomeTypeId,
                        (income, type) => new { income, type })
                    .ToListAsync();

                var incomeDtos = incomes.Select(i => new IncomeStatementDto
                {
                    IncomeId = i.income.IncomeId,
                    IncomeDate = i.income.IncomeDate,
                    IncomeTime = i.income.IncomeTime,
                    IncomeValue = i.income.IncomeValue,
                    IncomeTypeId = i.income.IncomeTypeId,
                    IncomeTypeName = i.type.IncomeTypeName,
                    IncomeDescription = i.income.IncomeDescription,
                    UserId = i.income.UserId,
                    UpdateDate = i.income.UpdateDate,
                    UpdateTime = i.income.UpdateTime
                }).ToList();

                return incomeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving income");
                throw;
            }
        }

        public async Task UpdateIncomeAsync(IncomeStatementDto incomeStatementDto)
        {
            try
            {
                var income = await _context.Incomes.FindAsync(incomeStatementDto.IncomeId);
                if (income == null)
                {
                    throw new Exception($"Income with ID {incomeStatementDto.IncomeId} not found");
                }
                income.IncomeDate = incomeStatementDto.IncomeDate;
                income.IncomeTime = incomeStatementDto.IncomeTime;
                income.IncomeValue = incomeStatementDto.IncomeValue;
                income.IncomeTypeId = incomeStatementDto.IncomeTypeId;
                income.IncomeDescription = incomeStatementDto.IncomeDescription;
                income.UserId = incomeStatementDto.UserId;
                income.UpdateDate = _utilService.GetThailandDate();
                income.UpdateTime = _utilService.GetThailandTime();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating income");
                throw;
            }
        }

        public async Task DeleteIncomeAsync(int incomeId)
        {
            try
            {
                var income = await _context.Incomes.FindAsync(incomeId);
                if (income == null)
                {
                    throw new Exception($"Income with ID {incomeId} not found");
                }
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting income");
                throw;
            }
        }
        public async Task<StatementStartValueDto> GetStatementStartValueAsync(DateOnly date)
        {
            try
            {
                var lastStatement = await _context.Statements
                    .Where(s => s.StatementDate <= date)
                    .OrderByDescending(s => s.StatementDate)
                    .ThenByDescending(s => s.StatementTime)
                    .FirstOrDefaultAsync();

                return new StatementStartValueDto
                {
                    StatementDate = lastStatement?.StatementDate,
                    StatementValue = lastStatement?.StatementValue ?? 0m
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statement start value");
                throw;
            }
        }

        public async Task<DailyReportSummaryDto> GetStatementSummaryAsync(SaleDateDto saleDateDto)
        {
            try
            {
                var now = System.DateTime.Today;
                
                // ✅ แปลง DateOnly? -> DateOnly ทันที
                var dateFrom = saleDateDto.DateFrom ?? new DateOnly(now.Year, now.Month, 1);
                var dateTo = saleDateDto.DateTo ?? new DateOnly(now.Year, now.Month, System.DateTime.DaysInMonth(now.Year, now.Month));

                // ✅ ส่ง dateFrom ที่เป็น DateOnly (non-nullable) เข้าไป
                var startStatement = await GetStatementStartValueAsync(dateFrom);
                var runningBalance = startStatement.StatementValue;

                // ✅ statementStartDate เป็น DateOnly (non-nullable)
                var statementStartDate = startStatement.StatementDate ?? dateFrom;

                // ✅ ปรับ saleDateDto ให้เป็น DateOnly? (nullable ได้เลย)
                saleDateDto.DateFrom = statementStartDate;
                saleDateDto.DateTo = dateTo;

                var dineInResult = await _ordersService.GetDailyDineInSalesReport(saleDateDto);
                var deliveryResult = await _ordersService.GetDailyDeliverySalesReport(saleDateDto);

                // ✅ statementStartDate และ dateTo เป็น DateOnly (non-nullable) ส่งได้เลย
                var dailyCostWithBankTransfer = await _costService.GetCostListbyPurchaseType(statementStartDate, dateTo, 1);
                var dailyCostWithCashPay = await _costService.GetCostListbyPurchaseType(statementStartDate, dateTo, 2);

                // ✅ GetIncomeAsync รับ DateOnly? จึงส่ง DateOnly เข้าไปได้เลย (implicit conversion)
                var incomeWithBankTransfer = await GetIncomeAsync(statementStartDate, dateTo, 1);
                var incomeWithCashPay = await GetIncomeAsync(statementStartDate, dateTo, 2);

                // ✅ 2. รวมวันที่ทั้งหมดในช่วง (ทุกวันแม้ไม่มีข้อมูลก็ยังแสดง)
                var dateSet = new HashSet<DateOnly>();

                // Generate ทุกวันในช่วง statementStartDate ถึง dateTo
                for (var d = statementStartDate; d <= dateTo; d = d.AddDays(1))
                    dateSet.Add(d);

                var allDates = dateSet.OrderBy(d => d).ToList();

                // ✅ 3. loop แต่ละวัน แล้ว map ข้อมูล
                var dailyStatements = new List<DailyStatementDto>();
                decimal previousBalance = runningBalance;

                foreach (var date in allDates)
                {
                    // ดึงข้อมูลยอดขายของวันนั้น
                    var dineIn   = dineInResult.FirstOrDefault(x => x.SaleDate == date);
                    var delivery = deliveryResult.FirstOrDefault(x => x.SaleDate == date);

                    // ดึงข้อมูลต้นทุนของวันนั้น
                    var bankCost = dailyCostWithBankTransfer.FirstOrDefault(x => x.CostDate == date);
                    var cashCost = dailyCostWithCashPay.FirstOrDefault(x => x.CostDate == date);

                    // ดึงข้อมูลรายรับของวันนั้น (Sum ถ้าไม่มีข้อมูลได้ 0 อัตโนมัติ)
                    var bankIncome = incomeWithBankTransfer.Where(x => x.IncomeDate == date).Sum(x => x.IncomeValue);
                    var cashIncome = incomeWithCashPay.Where(x => x.IncomeDate == date).Sum(x => x.IncomeValue);

                    // คำนวณยอดรวมแต่ละส่วน (ถ้าไม่มีข้อมูลได้ 0 จาก ?? 0)
                    var sales         = (dineIn?.TotalAmount   ?? 0) + (delivery?.TotalAmount ?? 0);
                    var transferCost  = bankCost?.TotalAmount  ?? 0;
                    var cashCostTotal = cashCost?.TotalAmount  ?? 0;
                    var totalCost     = transferCost + cashCostTotal;
                    var totalIncome   = bankIncome + cashIncome;

                    // Balance = ยอดก่อนหน้า + รายรับ - ต้นทุนเงินสด
                    runningBalance = previousBalance + totalIncome - cashCostTotal;

                    // Profit = Balance วันนี้ - Balance วันก่อนหน้า
                    var profit = runningBalance - previousBalance;

                    // Difference = ยอดขาย - ยอดเงินเข้า
                    var difference = sales - totalIncome;

                    dailyStatements.Add(new DailyStatementDto
                    {
                        Date         = date,
                        Sales        = sales,           // ยอดขายรวม (dineIn + delivery)
                        TotalCost    = totalCost,       // ต้นทุนรวม (โอน + เงินสด)
                        TransferCost = transferCost,    // ต้นทุนจ่ายโอน
                        CashCost     = cashCostTotal,   // ต้นทุนจ่ายเงินสด
                        TotalIncome  = totalIncome,     // รายรับรวม (โอน + เงินสด)
                        BankIncome   = bankIncome,      // รายรับโอนเข้าธนาคาร
                        CashIncome   = cashIncome,      // รายรับเงินสด
                        Balance      = runningBalance,  // ยอดเงินคงเหลือสะสม
                        Profit       = profit,          // กำไร (balance วันนี้ - วันก่อน)
                        Difference   = difference       // ส่วนต่าง (ยอดขาย - รายรับ)
                    });

                    previousBalance = runningBalance; // อัปเดตยอดก่อนหน้าสำหรับวันถัดไป
                }

                // ✅ 4. คำนวณยอดรวมทั้งหมด
                var summary = new DailyReportSummaryDto
                {
                    Balance           = runningBalance,
                    TotalBankAccount  = dailyStatements.Sum(x => x.BankIncome),
                    TotalCash         = dailyStatements.Sum(x => x.CashIncome),
                    TotalCost         = dailyStatements.Sum(x => x.TotalCost),
                    TotalTransferCost = dailyStatements.Sum(x => x.TransferCost),
                    TotalCashCost     = dailyStatements.Sum(x => x.CashCost),
                    TotalIncome       = dailyStatements.Sum(x => x.TotalIncome),
                    NetProfit         = dailyStatements.Sum(x => x.Profit),
                    HiddenCost        = 0,
                    DailyStatements   = dailyStatements
                };

                _logger.LogInformation(
                    "📊 StatementSummary | Balance: {Balance} | TotalIncome: {TotalIncome} | TotalCost: {TotalCost} | NetProfit: {NetProfit} | Days: {Days}",
                    summary.Balance, summary.TotalIncome, summary.TotalCost, summary.NetProfit, dailyStatements.Count);

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ เกิดข้อผิดพลาดในการดึงข้อมูล Statement Summary");
                throw;
            }
        }
    }
}