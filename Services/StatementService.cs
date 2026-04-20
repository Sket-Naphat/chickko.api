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
                    .Where(s => s.StatementDate <= date && s.Active)
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
                // 1) เตรียมช่วงวันที่รายงาน
                var now = System.DateTime.Today;
                var today = DateOnly.FromDateTime(now);
                var dateFrom = saleDateDto.DateFrom ?? new DateOnly(now.Year, now.Month, 1);
                var dateTo = saleDateDto.DateTo ?? new DateOnly(now.Year, now.Month, System.DateTime.DaysInMonth(now.Year, now.Month));
                var effectiveDateTo = dateTo > today ? today : dateTo;

                // 2) เตรียมยอดตั้งต้นและช่วงวันที่ที่จะใช้ query
                var startStatement = await GetStatementStartValueAsync(dateFrom);
                var runningBalance = startStatement.StatementValue;
                var statementStartDate = startStatement.StatementDate ?? dateFrom;
                saleDateDto.DateFrom = statementStartDate;
                saleDateDto.DateTo = effectiveDateTo;

                // 3) ดึงข้อมูลที่ใช้ประกอบรายงาน
                var dineInResult = await _ordersService.GetDailyDineInSalesReport(saleDateDto);
                var deliveryResult = await _ordersService.GetDailyDeliverySalesReport(saleDateDto);
                var dailyCostWithBankTransfer = await _costService.GetCostListbyPurchaseType(statementStartDate, effectiveDateTo, 1);
                var dailyCostWithCashPay = await _costService.GetCostListbyPurchaseType(statementStartDate, effectiveDateTo, 2);
                var dailyCostWithCashPayOut = await _costService.GetCostListbyPurchaseType(statementStartDate, effectiveDateTo, 3);
                var incomeWithBankTransfer = await GetIncomeAsync(statementStartDate, effectiveDateTo, 1);
                var incomeWithCashPay = await GetIncomeAsync(statementStartDate, effectiveDateTo, 2);

                // 4) สร้างรายการวันที่ทั้งหมดในช่วง (ทุกวันแม้ไม่มีข้อมูล)
                var allDates = new List<DateOnly>();
                for (var d = statementStartDate; d <= effectiveDateTo; d = d.AddDays(1))
                    allDates.Add(d);

                // 5) คำนวณข้อมูลรายวัน
                var dailyStatements = new List<DailyStatementDto>();
                var runningBalanceBank = runningBalance;
                decimal startingBalance = runningBalance;
                decimal runningBalanceCash = 0; // สมมติเริ่มต้นที่ 0 สำหรับเงินสด เพราะเราจะคำนวณแยกจากยอดรวมที่ได้จาก GetStatementStartValueAsync ซึ่งอาจจะรวมทั้งโอนและเงินสดแล้ว;
                decimal previousBalance = runningBalance;
                decimal previousBalanceBank = runningBalance;

                var ownerCost = await _costService.GetOwnerCost(statementStartDate, effectiveDateTo); // ดึงข้อมูลต้นทุนเจ้าของในช่วงเวลาที่รายงาน
                var totalOwnerCost = ownerCost.Sum(x => x.TotalAmount); // ยอดรวมต้นทุนเจ้าของในช่วงเวลาที่รายงาน
                // ✅ หาวันแรกที่เริ่มคิด ownerCost
                var ownerCostStartDate = ownerCost.Any()
                    ? ownerCost.Min(x => x.CostDate)
                    : (DateOnly?)null;

                decimal runningOwnerCost = 0; // ✅ สะสม ownerCost ตั้งแต่วันที่เริ่มต้น

                foreach (var date in allDates)
                {
                    var dineIn = dineInResult.FirstOrDefault(x => x.SaleDate == date);
                    var delivery = deliveryResult.FirstOrDefault(x => x.SaleDate == date);
                    var bankCost = dailyCostWithBankTransfer.FirstOrDefault(x => x.CostDate == date);
                    var cashCost = dailyCostWithCashPay.FirstOrDefault(x => x.CostDate == date);
                    var cashPayOut  = dailyCostWithCashPayOut.FirstOrDefault(x => x.CostDate == date); // ✅ ดึง cashPayOut ของวันนั้น
                    var bankIncome = incomeWithBankTransfer.Where(x => x.IncomeDate == date).Sum(x => x.IncomeValue);
                    var cashIncome = incomeWithCashPay.Where(x => x.IncomeDate == date).Sum(x => x.IncomeValue);

                    var sales = (dineIn?.TotalAmount ?? 0) + (delivery?.TotalAmount ?? 0);
                    var bankCostTotal = bankCost?.TotalAmount ?? 0;
                    var cashCostTotal = cashCost?.TotalAmount ?? 0;
                    var cashPayOutTotal = cashPayOut?.TotalAmount ?? 0;
                    var totalCost = bankCostTotal + cashCostTotal + cashPayOutTotal; // ✅ รวมต้นทุนจ่ายโอน, ต้นทุนจ่ายเงินสด และต้นทุนจ่ายเงินสดออกไปเข้าด้วยกันเพื่อให้เห็นภาพรวมของต้นทุนที่เกิดขึ้นในแต่ละวันได้ชัดเจนขึ้น เพราะบางวันอาจจะมีต้นทุนที่เกิดขึ้นในรูปแบบของการจ่ายเงินสดออกไปซึ่งไม่สะท้อนผ่านยอดเงินคงเหลือในบัญชีธนาคาร แต่ก็เป็นต้นทุนที่เกิดขึ้นจริงของธุรกิจ
                    var totalIncome = bankIncome + cashIncome;

                      // ✅ สะสม ownerCost เฉพาะตั้งแต่วันที่เริ่มคิดเป็นต้นไป
                    var ownerCostForDate = ownerCost
                        .Where(x => x.CostDate == date)
                        .Sum(x => x.TotalAmount);

                    if (ownerCostStartDate.HasValue && date >= ownerCostStartDate.Value)
                        runningOwnerCost += ownerCostForDate;


                    runningBalanceBank = previousBalanceBank + bankIncome - bankCostTotal;
                    runningBalanceCash = runningBalanceCash + cashIncome - cashPayOutTotal; // ✅ คำนวณยอดเงินคงเหลือเงินสดโดยเพิ่มรายรับเงินสดและหักต้นทุนจ่ายเงินสดออกไป ซึ่งจะช่วยให้เห็นภาพรวมของยอดเงินคงเหลือในส่วนของเงินสดได้ชัดเจนขึ้น เพราะรายรับและต้นทุนที่เกี่ยวข้องกับเงินสดจะถูกคำนวณแยกออกมาต่างหากจากยอดเงินคงเหลือในบัญชีธนาคารที่สะท้อนเฉพาะรายการที่เกี่ยวข้องกับการโอนเท่านั้น
                    runningBalance = runningBalanceBank + runningBalanceCash;
                    var profit = runningBalance - previousBalance - cashCostTotal; // กำไรของวันนี้ = ยอดเงินคงเหลือวันนี้ - ยอดเงินคงเหลือเมื่อวาน - ต้นทุนจ่ายเงินสดของวันนี้ ซึ่งจะช่วยให้เห็นภาพรวมของกำไรที่แท้จริงของแต่ละวันได้ชัดเจนขึ้น เพราะต้นทุนจ่ายเงินสดจะถูกหักออกจากกำไรทันทีในวันนั้นๆ ในขณะที่ต้นทุนจ่ายโอนจะสะท้อนผ่านยอดเงินคงเหลือในบัญชีธนาคารที่อัปเดตทุกวัน
                    var difference = (totalIncome + cashCostTotal) - sales; // ส่วนต่างระหว่างรายรับกับยอดขาย ซึ่งถ้าเป็นบวกแสดงว่ามีรายรับที่ไม่ได้มาจากยอดขาย (เช่น รายรับจากการคืนเงิน, รายรับจากแหล่งอื่น) แต่ถ้าเป็นลบแสดงว่ามียอดขายที่ไม่ได้ถูกบันทึกเป็นรายรับ (เช่น ขายแล้วแต่ยังไม่บันทึกรายรับ หรือมีการบันทึกรายรับน้อยกว่ายอดขายจริง) ซึ่งจะช่วยให้เห็นภาพรวมของธุรกิจได้ชัดเจนขึ้นว่า มีส่วนต่างระหว่างยอดขายกับรายรับมากน้อยแค่ไหน และอาจจะต้องตรวจสอบเพิ่มเติมในกรณีที่มีต้นทุนแฝงสูง

                    dailyStatements.Add(new DailyStatementDto
                    {
                        Date = date,
                        Sales = sales,           // ยอดขายรวม (dineIn + delivery)
                        TotalCost = totalCost,       // ต้นทุนรวม (โอน + เงินสด)
                        BankCost = bankCostTotal,    // ต้นทุนจ่ายโอน
                        CashCost = cashCostTotal + cashPayOutTotal,   // ต้นทุนจ่ายเงินสด
                        TotalIncome = totalIncome,     // รายรับรวม (โอน + เงินสด)
                        BankIncome = bankIncome,      // รายรับโอนเข้าธนาคาร
                        CashIncome = cashIncome,      // รายรับเงินสด
                        Profit = profit,          // กำไร (วันนี้ - วันก่อน)
                        Difference = difference,       // ส่วนต่าง (รายรับ - ยอดขาย)
                        Balance = runningBalanceBank + runningBalanceCash - runningOwnerCost,  // ยอดเงินคงเหลือสะสม
                        BankBalance = runningBalanceBank - runningOwnerCost, // ยอดเงินคงเหลือในบัญชีธนาคาร
                        CashBalance = runningBalanceCash // ยอดเงินคงเหลือเงินสด
                    });

                    previousBalance = runningBalance;
                    previousBalanceBank = runningBalanceBank;
                }

                //ดึงข้อมูลรายจ่ายคงค้าง (ต้นทุนที่เกิดขึ้นแต่ยังไม่จ่ายจริง) ในช่วงเวลาที่รายงาน
                var pendingCosts = await GetPendingCostsAsync(statementStartDate, effectiveDateTo);
                // 6) คำนวณยอดรวมระดับ summary
                
                var totalBank = dailyStatements.Sum(x => x.BankIncome);
                var totalCash = dailyStatements.Sum(x => x.CashIncome);
                var totalCostSum = dailyStatements.Sum(x => x.TotalCost) + totalOwnerCost;
                var totalBankCost = dailyStatements.Sum(x => x.BankCost);
                var totalCashCost = dailyStatements.Sum(x => x.CashCost);
                var totalCastPayOut = dailyCostWithCashPayOut.Sum(x => x.TotalAmount);
                var totalIncomeSum = dailyStatements.Sum(x => x.TotalIncome);
                var totalSales = dailyStatements.Sum(x => x.Sales);
                var hiddenCost = totalIncomeSum + (totalCashCost - totalCastPayOut) - totalSales;// ต้นทุนแฝง = รายรับรวม - ยอดขายรวม ซึ่งถ้าเป็นบวกแสดงว่ามีรายรับที่ไม่ได้มาจากยอดขาย (เช่น รายรับจากการคืนเงิน, รายรับจากแหล่งอื่น) แต่ถ้าเป็นลบแสดงว่ามียอดขายที่ไม่ได้ถูกบันทึกเป็นรายรับ (เช่น ขายแล้วแต่ยังไม่บันทึกรายรับ หรือมีการบันทึกรายรับน้อยกว่ายอดขายจริง) ซึ่งจะช่วยให้เห็นภาพรวมของธุรกิจได้ชัดเจนขึ้นว่า มีส่วนต่างระหว่างยอดขายกับรายรับมากน้อยแค่ไหน และอาจจะต้องตรวจสอบเพิ่มเติมในกรณีที่มีต้นทุนแฝงสูง
                var totalCostWithHidden = totalCostSum - hiddenCost; // ต้นทุนรวมถ้าคิดต้นทุนแฝงด้วย = ต้นทุนรวม + (ยอดขายรวม - รายรับรวม) ซึ่งจะสะท้อนภาพที่รัดกุมมากขึ้นว่า ถ้ารวมต้นทุนแฝงแล้ว ธุรกิจยังมีกำไรหรือขาดทุน
                // var netProfit = totalIncomeSum - totalCostWithHidden;
                var netProfit = totalIncomeSum - totalCostSum; // กำไรสุทธิ = รายรับรวม - ต้นทุนรวม (ไม่รวมต้นทุนแฝง) เพราะต้นทุนแฝงคือส่วนต่างระหว่างยอดขายกับรายรับ ซึ่งถ้านำมาคิดเป็นต้นทุนจะทำให้กำไรสุทธิลดลงมากเกินไปและไม่สะท้อนภาพจริงของธุรกิจที่อาจมีต้นทุนแฝงสูงแต่ยังมีกำไรได้ถ้ารายรับสูงกว่าต้นทุนจริงๆ
                var netProfitWithHidden = totalIncomeSum - totalCostWithHidden; // กำไรสุทธิถ้าคิดต้นทุนแฝงด้วย = รายรับรวม - (ต้นทุนรวม + ต้นทุนแฝง) ซึ่งจะสะท้อนภาพที่รัดกุมมากขึ้นว่า ถ้ารวมต้นทุนแฝงแล้ว ธุรกิจยังมีกำไรหรือขาดทุน
                runningBalanceBank -= totalOwnerCost; // ปรับยอดเงินคงเหลือในบัญชีธนาคารโดยหักต้นทุนเจ้าของออกไป เพราะต้นทุนเจ้าของคือค่าใช้จ่ายที่เกิดขึ้นจริงของธุรกิจ ถึงแม้จะไม่ได้จ่ายออกไปในรูปแบบเงินสดหรือโอน แต่ก็เป็นต้นทุนที่ควรนำมาคิดรวมในการวิเคราะห์กำไรขาดทุนเพื่อให้เห็นภาพที่รัดกุมมากขึ้น
                var summary = new DailyReportSummaryDto
                {
                    Balance = runningBalanceBank + runningBalanceCash , // ยอดเงินคงเหลือรวม
                    TotalBank = totalBank , // เงินบัญชีธนาคารรวมที่หักต้นทุนเจ้าของออกไปแล้ว เพราะต้นทุนเจ้าของคือค่าใช้จ่ายที่เกิดขึ้นจริงของธุรกิจ ถึงแม้จะไม่ได้จ่ายออกไปในรูปแบบเงินสดหรือโอน แต่ก็เป็นต้นทุนที่ควรนำมาคิดรวมในการวิเคราะห์กำไรขาดทุนเพื่อให้เห็นภาพที่รัดกุมมากขึ้น
                    TotalCash = totalCash,
                    TotalCost = totalCostSum, // ต้นทุนรวมที่รวมต้นทุนเจ้าของด้วย เพราะต้นทุนเจ้าของคือค่าใช้จ่ายที่เกิดขึ้นจริงของธุรกิจ ถึงแม้จะไม่ได้จ่ายออกไปในรูปแบบเงินสดหรือโอน แต่ก็เป็นต้นทุนที่ควรนำมาคิดรวมในการวิเคราะห์กำไรขาดทุนเพื่อให้เห็นภาพที่รัดกุมมากขึ้น
                    TotalBankCost = totalBankCost,
                    TotalCashCost = totalCashCost,
                    TotalIncome = totalIncomeSum,
                    NetProfit = netProfit, // รายรับรวม - ต้นทุนรวม (ไม่รวมต้นทุนแฝง)
                    HiddenCost = hiddenCost, // ต้นทุนแฝง = ยอดขาย - รายรับ
                    TotalSales = totalSales,
                    BankBalance = (dailyStatements.LastOrDefault()?.BankBalance ?? 0), // ยอดเงินคงเหลือในบัญชีธนาคาร ณ วันสุดท้าย
                    CashBalance = dailyStatements.LastOrDefault()?.CashBalance ?? 0, // ยอดเงินคงเหลือในเงินสด ณ วันสุดท้าย
                    DailyStatements = dailyStatements,
                    TotalCostWithHidden = totalCostWithHidden,
                    StartingBalance = startingBalance, // เงินตั้งต้น
                    netProfitWithHidden = netProfitWithHidden, // กำไรสุทธิถ้าคิดต้นทุนแฝงด้วย
                    NetChange = (runningBalanceBank + runningBalanceCash) - startingBalance, // การเปลี่ยนแปลงของยอดเงินคงเหลือ = ยอดเงินคงเหลือ ณ วันสุดท้าย - เงินตั้งต้น ซึ่งจะช่วยให้เห็นภาพรวมของการเปลี่ยนแปลงของยอดเงินคงเหลือในช่วงเวลาที่รายงานได้ชัดเจนขึ้น
                    PendingCost = pendingCosts, // รายจ่ายคงค้างในช่วงเวลาที่รายงาน
                    TotalOwnerCost = totalOwnerCost // ต้นทุนเจ้าของรวมในช่วงเวลาที่รายงาน
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

        private async Task<decimal> GetPendingCostsAsync(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // ดึงค่าแรงคงค้าง (IsPurchase == false) ของพนักงาน (ไม่รวม owner)
                var totalPendingWage = await _context.Worktime
                    .Include(w => w.Employee)
                    .Where(w => w.WorkDate >= startDate
                             && w.WorkDate <= endDate
                             && w.IsPurchase == false
                             && w.Employee != null
                             && w.Employee.UserPermistionID != 1) // ไม่รวม owner
                    .SumAsync(w => (decimal)w.WageCost);

                // ดึงรายจ่ายคงค้างจาก Cost (IsPurchase == false) ในช่วงวันที่เดียวกัน
                var totalPendingCost = await _context.Cost
                    .Where(c => c.CostDate >= startDate
                             && c.CostDate <= endDate
                             && c.IsPurchase == false
                             && c.IsActive == true)
                    .SumAsync(c => (decimal)c.CostPrice);

                return totalPendingWage + totalPendingCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending costs");
                throw;
            }
        }
    }
}