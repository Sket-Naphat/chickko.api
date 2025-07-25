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

        public WorktimeService(ChickkoContext context, ILogger<WorktimeService> logger)
        {
            _context = context;
            _logger = logger;
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
                    return "มีการลงเวลาครั้งก่อนหน้าแล้วเมื่อ :"+_Worktime.TimeClockIn +" !";
                }
                else
                {
                    var Worktime = new Worktime
                    {
                        EmployeeID = WorktimeDto.EmployeeID,
                        WorkDate = workDate,
                        TimeClockIn = timeClockIn,
                        Active = true,
                        IsPurchese = false,
                        ClockInLocation = "",
                        TotalWorktime = 0,
                        UpdateDate = DateOnly.FromDateTime(DateTime.Now),
                        UpdateTime = TimeOnly.FromDateTime(DateTime.Now),
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
                var _Employee = await _context.Users.Include(u => u.UserPermistion).FirstOrDefaultAsync(e => e.Id == WorktimeDto.EmployeeID);


                if (_Worktime != null && _Employee != null && _Employee.UserPermistion != null)
                {
                    // _Worktime.EmployeeID = WorktimeDto.EmployeeID;
                    _Worktime.TimeClockOut = timeClockOut;
                    _Worktime.ClockInLocation = "";
                    _Worktime.TotalWorktime = 0;
                    _Worktime.UpdateDate = DateOnly.FromDateTime(DateTime.Now);
                    _Worktime.UpdateTime = TimeOnly.FromDateTime(DateTime.Now);


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

                return "บันทึกเวลาออกจากงานสำเร็จเมื่อ "+ timeClockOut +" !";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetCurrentStock: {ex.Message}");

                // หรือโยนต่อไปให้ controller จัดการ (ถ้าคุณใช้ error middleware อยู่)
                throw;
            }

        }
    }
}