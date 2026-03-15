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
        public StatementService(ChickkoContext context, ILogger<StatementService> logger, IUtilService utilService)
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
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

        public async Task<List<IncomeStatementDto>> GetIncomeAsync(DateOnly? dateFrom, DateOnly? dateTo)
        {
            try
            {
                // ถ้าไม่ได้ส่งช่วงวันที่มา ให้ default เป็นเดือนปัจจุบัน
                var now = DateTime.Now;
                var firstDay = new DateOnly(now.Year, now.Month, 1);
                var lastDay = new DateOnly(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                dateFrom ??= firstDay;
                dateTo ??= lastDay;

                var incomes = await _context.Incomes
                    .Where(i => i.IncomeDate >= dateFrom && i.IncomeDate <= dateTo)
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
    }
}