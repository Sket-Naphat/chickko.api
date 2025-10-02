using System.Globalization;
using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services
{
    public class WorktimeService : IWorktimeService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<WorktimeService> _logger;
        private readonly IUtilService _utilService;

        public WorktimeService(ChickkoContext context, ILogger<WorktimeService> logger, IUtilService utilService)
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
        }
        public async Task<string> ClockIn(WorktimeDto WorktimeDto)
        {
            try
            {

                var workDate = DateOnly.TryParseExact(WorktimeDto.WorkDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                TimeOnly? timeClockIn = string.IsNullOrEmpty(WorktimeDto.TimeClockIn)
                    ? null
                    : TimeOnly.ParseExact(WorktimeDto.TimeClockIn, "HH:mm:ss");

                var _Worktime = await _context.Worktime.FirstOrDefaultAsync(w => w.WorkDate == workDate && w.EmployeeID == WorktimeDto.EmployeeID);
                if (_Worktime != null)
                {
                    return "มีการลงเวลาครั้งก่อนหน้าแล้วเมื่อ :" + _Worktime.TimeClockIn + " !";
                }
                else
                {
                    var Worktime = new Worktime
                    {
                        EmployeeID = WorktimeDto.EmployeeID,
                        WorkDate = workDate,
                        TimeClockIn = timeClockIn,
                        Active = true,
                        IsPurchase = false,
                        ClockInLocation = WorktimeDto.ClockInLocation ?? string.Empty,
                        TotalWorktime = 0,
                        // UpdateDate = DateOnly.FromDateTime(DateTime.Now),
                        // UpdateTime = TimeOnly.FromDateTime(DateTime.Now),
                    };


                    _context.Worktime.Add(Worktime);
                    await _context.SaveChangesAsync();

                    return "บันทึกเวลาเข้างานสำเร็จ !";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetCurrentStock: {ex.Message}");

                // หรือโยนต่อไปให้ controller จัดการ (ถ้าคุณใช้ error middleware อยู่)
                throw;
            }

        }

        public async Task<string> ClockOut(WorktimeDto WorktimeDto)
        {
            try
            {
                // สมมุติว่า WorktimeDto คือ DTO ที่รับมาจาก client
                var workDate = DateOnly.TryParseExact(WorktimeDto.WorkDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                TimeOnly? timeClockIn = string.IsNullOrEmpty(WorktimeDto.TimeClockIn)
                             ? null
                             : TimeOnly.ParseExact(WorktimeDto.TimeClockIn, "HH:mm:ss");

                TimeOnly? timeClockOut = string.IsNullOrEmpty(WorktimeDto.TimeClockOut)
                    ? null
                    : TimeOnly.ParseExact(WorktimeDto.TimeClockOut, "HH:mm:ss");


                var _Worktime = await _context.Worktime.FirstOrDefaultAsync(w => w.WorkDate == workDate && w.EmployeeID == WorktimeDto.EmployeeID);
                var _Employee = await _context.Users.Include(u => u.UserPermistion).FirstOrDefaultAsync(e => e.UserId == WorktimeDto.EmployeeID);


                if (_Worktime != null && _Employee != null && _Employee.UserPermistion != null)
                {
                    // _Worktime.EmployeeID = WorktimeDto.EmployeeID;
                    _Worktime.TimeClockOut = timeClockOut;
                    _Worktime.ClockOutLocation = WorktimeDto.ClockOutLocation ?? string.Empty;
                    _Worktime.TotalWorktime = 0;
                    // _Worktime.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    // _Worktime.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);


                    if (_Worktime?.TimeClockIn != null && _Worktime.TimeClockOut != null)
                    {
                        var clockIn = _Worktime.TimeClockIn.Value.ToTimeSpan();
                        var clockOut = _Worktime.TimeClockOut.Value.ToTimeSpan();

                        if (clockOut < clockIn)
                        {
                            clockOut = clockOut.Add(TimeSpan.FromDays(1));
                        }


                        // var totalHours = (clockOut - clockIn).TotalHours;
                        var totalHours = Math.Round((clockOut - clockIn).TotalHours, 2);
                        _Worktime.TotalWorktime = totalHours;
                        double CostWage = 0;
                        if (_Employee.UserPermistion.UserPermistionID == 1) //owner 
                        {
                            CostWage = _Employee.UserPermistion.WageCost;
                        }
                        else
                        {
                            CostWage = totalHours * _Employee.UserPermistion.WageCost; //employee
                        }
                        // _Worktime.WageCost = CostWage;
                        _Worktime.WageCost = Math.Ceiling(CostWage);
                        await _context.SaveChangesAsync();
                    }

                    // บันทึกกลับฐานข้อมูล
                    await _context.SaveChangesAsync();
                }

                return "บันทึกเวลาออกจากงานสำเร็จเมื่อ " + timeClockOut + " !";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetCurrentStock: {ex.Message}");

                // หรือโยนต่อไปให้ controller จัดการ (ถ้าคุณใช้ error middleware อยู่)
                throw;
            }

        }
        public async Task<List<Worktime>> GetAllPeriodWorktime()
        {
            try
            {
                var worktimes = await _context.Worktime
                    .Include(w => w.Employee) // รวมข้อมูลพนักงาน
                    .OrderByDescending(w => w.WorkDate) // เรียงตามวันที่ทำงานจากใหม่ไปเก่า
                    .ThenByDescending(w => w.TimeClockIn) // เรียงตามเวลาเข้างานจากใหม่ไปเก่า
                    .ToListAsync();

                return worktimes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllPeriodWorktime: {ex.Message}");
                throw;
            }
        }
        public async Task<WorktimeDto> GetPeriodWorktimeByEmployeeID(WorktimeDto _Worktime)
        {
            try
            {
                var workDate = DateOnly.TryParseExact(_Worktime.WorkDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    ? parsedDate
                    : DateOnly.FromDateTime(DateTime.Now);

                var worktimes = await _context.Worktime
                    .Include(w => w.Employee) // รวมข้อมูลพนักงาน
                    .Where(w => w.EmployeeID == _Worktime.EmployeeID && w.WorkDate == workDate)
                    .OrderByDescending(w => w.WorkDate) // เรียงตามวันที่ทำงานจากใหม่ไปเก่า
                    .ThenByDescending(w => w.TimeClockIn) // เรียงตามเวลาเข้างานจากใหม่ไปเก่า
                    .FirstOrDefaultAsync();
                if (worktimes == null)
                {
                    return new WorktimeDto(); // หรือจัดการกรณีที่ไม่พบข้อมูลตามต้องการ
                }

                _Worktime = new WorktimeDto
                {
                    EmployeeID = worktimes.EmployeeID,
                    WorkDate = worktimes.WorkDate.ToString("yyyy-MM-dd"),
                    TimeClockIn = worktimes.TimeClockIn?.ToString("HH:mm:ss"),
                    TimeClockOut = worktimes.TimeClockOut?.ToString("HH:mm:ss"),
                    ClockInLocation = worktimes.ClockInLocation,
                    ClockOutLocation = worktimes.ClockOutLocation,
                };
                return _Worktime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetPeriodWorktimeByEmployeeID: {ex.Message}");
                throw;
            }
        }
        public async Task<List<WorktimeDto>> GetWorkTimeHistoryByEmployeeID(WorktimeDto worktimeDto)
        {
            try
            {
                int year = !string.IsNullOrEmpty(worktimeDto.WorkYear) ? int.Parse(worktimeDto.WorkYear) : DateTime.Now.Year;
                int month = !string.IsNullOrEmpty(worktimeDto.WorkMonth) ? int.Parse(worktimeDto.WorkMonth) : DateTime.Now.Month;

                var worktimes = await _context.Worktime
                    .Include(w => w.Employee)
                    .Where(w => w.EmployeeID == worktimeDto.EmployeeID
                             && w.WorkDate.Year == year
                             && w.WorkDate.Month == month)    // ✅ filter เดือนตรง ๆ
                    .OrderByDescending(w => w.WorkDate)
                    .ThenByDescending(w => w.TimeClockIn)
                    .ToListAsync();

                var worktimeDtos = worktimes.Select(w => new WorktimeDto
                {
                    WorktimeID = w.WorktimeID,
                    EmployeeID = w.EmployeeID,
                    EmployeeName = w.Employee?.Name ?? "",
                    WorkDate = w.WorkDate.ToString("yyyy-MM-dd"),
                    TimeClockIn = w.TimeClockIn?.ToString("HH:mm:ss"),
                    TimeClockOut = w.TimeClockOut?.ToString("HH:mm:ss"),
                    ClockInLocation = w.ClockInLocation,
                    ClockOutLocation = w.ClockOutLocation,
                    TotalWorktime = w.TotalWorktime,
                    WageCost = w.WageCost,
                    Bonus = w.Bonus,
                    Price = w.Price,
                    IsPurchase = w.IsPurchase,
                    Remark = w.Remark,
                    Active = w.Active
                }).ToList();

                return worktimeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetWorkTimeHistoryByEmployeeID: {ex.Message}");
                throw;
            }
        }
        public async Task<List<WorktimeDto>> GetWorkTimeHistoryByPeriod(WorktimeDto worktimeDto)
        {
            try
            {
                int year = !string.IsNullOrEmpty(worktimeDto.WorkYear) ? int.Parse(worktimeDto.WorkYear) : DateTime.Now.Year;
                int month = !string.IsNullOrEmpty(worktimeDto.WorkMonth) ? int.Parse(worktimeDto.WorkMonth) : DateTime.Now.Month;

                DateOnly startDate;
                if (DateOnly.TryParseExact(worktimeDto.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
                    startDate = parsedStartDate;
                else if (int.TryParse(worktimeDto.StartDate, out var day))
                    startDate = new DateOnly(year, month, day);
                else
                    startDate = new DateOnly(year, month, 1);

                DateOnly endDate;
                if (DateOnly.TryParseExact(worktimeDto.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
                    endDate = parsedEndDate;
                else if (int.TryParse(worktimeDto.EndDate, out var endDay))
                    endDate = new DateOnly(year, month, endDay);
                else
                    endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

                var worktimes = await _context.Worktime
                    .Include(w => w.Employee)
                    .Where(w => w.WorkDate >= startDate
                            && w.WorkDate <= endDate
                            && w.Employee != null
                            && w.Employee.UserPermistionID != 1) // Exclude owners
                    .OrderByDescending(w => w.WorkDate)
                    .ThenByDescending(w => w.TimeClockIn)
                    .ToListAsync();

                // สรุปข้อมูลรวมของแต่ละคน
                var summary = worktimes
                    .GroupBy(w => new { w.EmployeeID, Name = w.Employee?.Name ?? "" })
                    .Select(g => new
                    {
                        EmployeeID = g.Key.EmployeeID,
                        EmployeeName = g.Key.Name,
                        TotalWorktime = g.Sum(x => x.TotalWorktime),
                        TotalPrice = g.Sum(x => x.Price),
                        TotalWageCost = g.Sum(x => x.WageCost),
                        Details = g.Select(w => new WorktimeDto
                        {
                            WorktimeID = w.WorktimeID,
                            EmployeeID = w.EmployeeID,
                            EmployeeName = w.Employee?.Name ?? "",
                            WorkDate = w.WorkDate.ToString("yyyy-MM-dd"),
                            TimeClockIn = w.TimeClockIn?.ToString("HH:mm:ss"),
                            TimeClockOut = w.TimeClockOut?.ToString("HH:mm:ss"),
                            ClockInLocation = w.ClockInLocation,
                            ClockOutLocation = w.ClockOutLocation,
                            TotalWorktime = w.TotalWorktime,
                            WageCost = w.WageCost,
                            Bonus = w.Bonus,
                            Price = w.Price,
                            IsPurchase = w.IsPurchase,
                            Remark = w.Remark,
                            Active = w.Active
                        }).ToList()
                    }).ToList();

                var result = summary.Select(s => new WorktimeDto
                {
                    EmployeeID = s.EmployeeID,
                    EmployeeName = s.EmployeeName,
                    TotalWorktime = s.TotalWorktime,
                    Price = s.TotalPrice,
                    WageCost = s.TotalWageCost,
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetWorkTimeHistoryByPeriod: {ex.Message}");
                throw;
            }
        }
        public async Task<WorktimeSummaryDto> GetWorkTimeCostByEmployeeIDandPeriod(WorktimeDto worktimeDto)
        {
            try
            {
                int year = !string.IsNullOrEmpty(worktimeDto.WorkYear) ? int.Parse(worktimeDto.WorkYear) : DateTime.Now.Year;
                int month = !string.IsNullOrEmpty(worktimeDto.WorkMonth) ? int.Parse(worktimeDto.WorkMonth) : DateTime.Now.Month;

                DateOnly startDate;
                if (DateOnly.TryParseExact(worktimeDto.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
                    startDate = parsedStartDate;
                else if (int.TryParse(worktimeDto.StartDate, out var day))
                    startDate = new DateOnly(year, month, day);
                else
                    startDate = new DateOnly(year, month, 1);

                DateOnly endDate;
                if (DateOnly.TryParseExact(worktimeDto.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
                    endDate = parsedEndDate;
                else if (int.TryParse(worktimeDto.EndDate, out var endDay))
                    endDate = new DateOnly(year, month, endDay);
                else
                    endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

                var worktimes = await _context.Worktime
                    .Include(w => w.Employee)
                    .Where(w => w.WorkDate >= startDate
                            && w.WorkDate <= endDate
                            && w.Employee != null
                            && w.Employee.UserPermistionID != 1 // Exclude owners
                            && w.EmployeeID == worktimeDto.EmployeeID
                           ) // เฉพาะพนักงานที่ระบุและยังไม่ถูกบันทึกเป็นค่าใช้จ่าย
                    .OrderByDescending(w => w.WorkDate)
                    .ThenByDescending(w => w.TimeClockIn)
                    .ToListAsync();

                // สรุปข้อมูลรวมของแต่ละคน
                var summary = worktimes
                    .GroupBy(w => new { w.EmployeeID, Name = w.Employee?.Name ?? "" })
                    .Select(g => new
                    {
                        EmployeeID = g.Key.EmployeeID,
                        EmployeeName = g.Key.Name,
                        TotalWorktime = g.Where(x => x.IsPurchase == false).Sum(x => x.TotalWorktime),
                        // TotalWageCost = g.Sum(x => x.WageCost),
                        // ✅ แก้ไข: คิดเฉพาะรายการที่ยังไม่จ่าย (IsPurchase == false)
                        TotalWageCost = g.Where(x => x.IsPurchase == false).Sum(x => x.WageCost),

                        Details = g.Select(w => new WorktimeDto
                        {
                            WorktimeID = w.WorktimeID,
                            EmployeeID = w.EmployeeID,
                            EmployeeName = w.Employee?.Name ?? "",
                            WorkDate = w.WorkDate.ToString("yyyy-MM-dd"),
                            TimeClockIn = w.TimeClockIn?.ToString("HH:mm:ss"),
                            TimeClockOut = w.TimeClockOut?.ToString("HH:mm:ss"),
                            ClockInLocation = w.ClockInLocation,
                            ClockOutLocation = w.ClockOutLocation,
                            TotalWorktime = w.TotalWorktime,
                            WageCost = w.WageCost,
                            Bonus = w.Bonus,
                            Price = w.Price,
                            IsPurchase = w.IsPurchase,
                            Remark = w.Remark,
                            Active = w.Active
                        }).ToList()
                    }).ToList();

                var result = summary.Select(s => new WorktimeSummaryDto
                {
                    EmployeeID = s.EmployeeID,
                    EmployeeName = s.EmployeeName,
                    TotalWorktime = s.TotalWorktime,
                    WageCost = s.TotalWageCost,
                    Worktimes = s.Details.ToList() // เฉพาะรายการที่ยังไม่ถูกบันทึกเป็นค่าใช้จ่าย
                    // Worktimes = s.Details.Where(d => d.IsPurchase == true).ToList() // เฉพาะรายการที่ยังไม่ถูกบันทึกเป็นค่าใช้จ่าย
                }).FirstOrDefault();

                return result ?? new WorktimeSummaryDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetWorkTimeHistoryByEmployeeID: {ex.Message}");
                throw;
            }
        }
        public async Task<string> UpdateTimeClockIn(WorktimeDto worktimeDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(worktimeDto.TimeClockIn) && !string.IsNullOrEmpty(worktimeDto.WorkDate))
                {
                    var parsed = DateOnly.TryParseExact(worktimeDto.WorkDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate);
                    DateOnly WorkDate = parsed ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                    TimeOnly? timeClockIn = TimeOnly.ParseExact(worktimeDto.TimeClockIn, "HH:mm:ss");

                    var _Worktime = await _context.Worktime.FirstOrDefaultAsync(w => w.WorkDate == WorkDate && w.EmployeeID == worktimeDto.EmployeeID);
                    var _Employee = await _context.Users.Include(u => u.UserPermistion).FirstOrDefaultAsync(e => e.UserId == worktimeDto.EmployeeID);
                    if (_Worktime == null)
                    {
                        return "ไม่พบข้อมูลการลงเวลาของพนักงานในวันที่ระบุ !";
                    }
                    else
                    {
                        if (_Worktime.TimeClockOut != null)
                        {
                            // ถ้ามีเวลาออกงานแล้ว คำนวณเวลาทำงานใหม่

                            var clockIn = timeClockIn.Value.ToTimeSpan();
                            var clockOut = _Worktime.TimeClockOut.Value.ToTimeSpan();

                            if (clockOut < clockIn)
                            {
                                clockOut = clockOut.Add(TimeSpan.FromDays(1));
                            }

                            // var totalHours = (clockOut - clockIn).TotalHours;
                            var totalHours = Math.Round((clockOut - clockIn).TotalHours, 2);
                            _Worktime.TotalWorktime = totalHours;
                            double CostWage = 0;

                            if (_Employee != null && _Employee.UserPermistion != null && _Employee.UserPermistion.UserPermistionID == 1) //owner
                            {
                                CostWage = _Employee.UserPermistion.WageCost;
                            }
                            else if (_Employee != null && _Employee.UserPermistion != null)
                            {
                                CostWage = totalHours * _Employee.UserPermistion.WageCost; //employee
                            }
                            else
                            {
                                CostWage = 0; // or handle the case where UserPermistion is null
                            }
                            // _Worktime.WageCost = CostWage;
                            _Worktime.WageCost = Math.Ceiling(CostWage);

                        }

                        _Worktime.TimeClockIn = timeClockIn;
                        _Worktime.UpdateDate = _utilService.GetThailandDate();
                        _Worktime.UpdateTime = _utilService.GetThailandTime();
                        _Worktime.UpdateBy = worktimeDto.CreatedBy;

                        await _context.SaveChangesAsync();

                        return "อัปเดตเวลาเข้างานสำเร็จ !";
                    }
                }
                else
                {
                    return "ข้อมูลเวลาเข้างานไม่ถูกต้อง !";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateTimeClockIn: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UpdateTimeClockOut(WorktimeDto worktimeDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(worktimeDto.TimeClockOut) && !string.IsNullOrEmpty(worktimeDto.WorkDate))
                {
                    var parsed = DateOnly.TryParseExact(worktimeDto.WorkDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate);
                    DateOnly WorkDate = parsed ? orderDate : DateOnly.FromDateTime(DateTime.Now);
                    TimeOnly? timeClockOut = TimeOnly.ParseExact(worktimeDto.TimeClockOut, "HH:mm:ss");

                    var _Worktime = await _context.Worktime.FirstOrDefaultAsync(w => w.WorkDate == WorkDate && w.EmployeeID == worktimeDto.EmployeeID);
                    var _Employee = await _context.Users.Include(u => u.UserPermistion).FirstOrDefaultAsync(e => e.UserId == worktimeDto.EmployeeID);
                    if (_Worktime == null)
                    {
                        return "ไม่พบข้อมูลการลงเวลาของพนักงานในวันที่ระบุ !";
                    }
                    else
                    {
                        if (_Worktime.TimeClockIn != null)
                        {
                            // ถ้ามีเวลาออกงานแล้ว คำนวณเวลาทำงานใหม่

                            var clockIn = _Worktime.TimeClockIn.Value.ToTimeSpan();
                            var clockOut = timeClockOut.Value.ToTimeSpan();

                            if (clockOut < clockIn)
                            {
                                clockOut = clockOut.Add(TimeSpan.FromDays(1));
                            }

                            // var totalHours = (clockOut - clockIn).TotalHours;
                            var totalHours = Math.Round((clockOut - clockIn).TotalHours, 2);
                            _Worktime.TotalWorktime = totalHours;
                            double CostWage = 0;

                            if (_Employee != null && _Employee.UserPermistion != null && _Employee.UserPermistion.UserPermistionID == 1) //owner
                            {
                                CostWage = _Employee.UserPermistion.WageCost;
                            }
                            else if (_Employee != null && _Employee.UserPermistion != null)
                            {
                                CostWage = totalHours * _Employee.UserPermistion.WageCost; //employee
                            }
                            else
                            {
                                CostWage = 0; // or handle the case where UserPermistion is null
                            }
                            // _Worktime.WageCost = CostWage;
                            _Worktime.WageCost = Math.Ceiling(CostWage);

                        }

                        _Worktime.TimeClockOut = timeClockOut;
                        _Worktime.UpdateDate = _utilService.GetThailandDate();
                        _Worktime.UpdateTime = _utilService.GetThailandTime();
                        _Worktime.UpdateBy = worktimeDto.CreatedBy;

                        await _context.SaveChangesAsync();

                        return "อัปเดตเวลาออกงานสำเร็จ !";
                    }
                }
                else
                {
                    return "ข้อมูลเวลาออกงานไม่ถูกต้อง !";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateTimeClockIn: {ex.Message}");
                throw;
            }
        }
    }
}