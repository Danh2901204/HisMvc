using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppointmentsApiController> _logger;

    public AppointmentsApiController(AppDbContext db, ILogger<AppointmentsApiController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/AppointmentsApi/AvailableSlots?date=2026-01-09&departmentId=1
    [HttpGet("AvailableSlots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] DateOnly date, [FromQuery] int? departmentId)
    {
        try
        {
            // Lay danh sach khung gio
            var timeSlots = await _db.TimeSlots
                .OrderBy(x => x.Start)
                .ToListAsync();

            // Lay danh sach appointment da dat trong ngay
            var bookedAppointments = await _db.Appointments
                .Where(x => x.Date == date && x.Status == AppointmentStatus.Booked)
                .ToListAsync();

            // Tinh so luong con trong moi khung gio (gia su moi khung gio cho phep 10 appointment)
            var availableSlots = timeSlots.Select(slot => new
            {
                slot.TimeSlotId,
                slot.Code,
                Start = slot.Start.ToString("HH:mm"),
                End = slot.End.ToString("HH:mm"),
                Booked = bookedAppointments.Count(a => a.TimeSlotId == slot.TimeSlotId),
                MaxCapacity = 10,
                Available = 10 - bookedAppointments.Count(a => a.TimeSlotId == slot.TimeSlotId)
            }).Where(x => x.Available > 0).ToList();

            return Ok(new
            {
                success = true,
                date = date.ToString("yyyy-MM-dd"),
                slots = availableSlots
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available slots");
            return StatusCode(500, new { success = false, message = "Loi server" });
        }
    }

    // GET: api/AppointmentsApi/Departments
    [HttpGet("Departments")]
    public async Task<IActionResult> GetDepartments()
    {
        try
        {
            var departments = await _db.Departments
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.DepartmentId,
                    x.Code,
                    x.Name
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                departments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return StatusCode(500, new { success = false, message = "Loi server" });
        }
    }

    // GET: api/AppointmentsApi/Doctors?departmentId=1
    [HttpGet("Doctors")]
    public async Task<IActionResult> GetDoctors([FromQuery] int? departmentId)
    {
        try
        {
            var query = _db.Staffs
                .Where(x => x.StaffType == "DOCTOR")
                .AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

            var doctors = await query
                .OrderBy(x => x.FullName)
                .Select(x => new
                {
                    x.StaffId,
                    x.FullName,
                    x.DepartmentId,
                    DepartmentName = x.Department!.Name
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                doctors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctors");
            return StatusCode(500, new { success = false, message = "Loi server" });
        }
    }

    // POST: api/AppointmentsApi/Book
    [HttpPost("Book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
            {
                return BadRequest(new { success = false, message = "Vui long nhap day du thong tin" });
            }

            // Tim hoac tao benh nhan
            var patient = await _db.Patients.FirstOrDefaultAsync(x => x.Phone == request.Phone);
            if (patient == null)
            {
                patient = new Patient
                {
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Dob = request.Dob,
                    Gender = request.Gender ?? Gender.Unknown
                };
                _db.Patients.Add(patient);
                await _db.SaveChangesAsync();
            }
            else
            {
                // Cap nhat thong tin
                patient.FullName = request.FullName;
                if (request.Dob.HasValue) patient.Dob = request.Dob;
                if (request.Gender.HasValue) patient.Gender = request.Gender.Value;
                await _db.SaveChangesAsync();
            }

            // Kiem tra khung gio con trong
            var existingCount = await _db.Appointments
                .CountAsync(x => x.Date == request.Date && 
                                 x.TimeSlotId == request.TimeSlotId && 
                                 x.Status == AppointmentStatus.Booked);

            if (existingCount >= 10)
            {
                return BadRequest(new { success = false, message = "Khung gio da day, vui long chon khung gio khac" });
            }

            // Tao ma lich hen
            var code = $"APT{DateTime.Now:yyyyMMddHHmmss}";

            // Tao appointment
            var appointment = new Appointment
            {
                Code = code,
                PatientId = patient.PatientId,
                DepartmentId = request.DepartmentId,
                DoctorId = request.DoctorId,
                Date = request.Date,
                TimeSlotId = request.TimeSlotId,
                Status = AppointmentStatus.Booked,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Dat lich thanh cong",
                appointmentCode = code,
                appointmentId = appointment.AppointmentId,
                date = request.Date.ToString("dd/MM/yyyy")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment");
            return StatusCode(500, new { success = false, message = "Loi server khi dat lich" });
        }
    }

    // GET: api/AppointmentsApi/Check?code=APT20260109123456
    [HttpGet("Check")]
    public async Task<IActionResult> CheckAppointment([FromQuery] string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { success = false, message = "Ma lich hen khong hop le" });
            }

            var appointment = await _db.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Department)
                .Include(x => x.Doctor)
                .Include(x => x.TimeSlot)
                .FirstOrDefaultAsync(x => x.Code == code);

            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Khong tim thay lich hen" });
            }

            return Ok(new
            {
                success = true,
                appointment = new
                {
                    appointment.Code,
                    appointment.Status,
                    Date = appointment.Date.ToString("dd/MM/yyyy"),
                    TimeSlot = $"{appointment.TimeSlot?.Start:HH:mm} - {appointment.TimeSlot?.End:HH:mm}",
                    Patient = new
                    {
                        appointment.Patient?.FullName,
                        appointment.Patient?.Phone,
                        appointment.Patient?.Gender
                    },
                    Department = appointment.Department?.Name,
                    Doctor = appointment.Doctor?.FullName,
                    appointment.Note
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking appointment");
            return StatusCode(500, new { success = false, message = "Loi server" });
        }
    }
}

// DTO cho request dat lich
public class BookAppointmentRequest
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public Gender? Gender { get; set; }
    public int DepartmentId { get; set; }
    public int? DoctorId { get; set; }
    public DateOnly Date { get; set; }
    public int TimeSlotId { get; set; }
    public string? Note { get; set; }
}
