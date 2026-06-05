using HisMvc.Models;
using HisMvc.Models.Chatbot;
using HisMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace HisMvc.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsApiController : ControllerBase
{
    private readonly IPublicAppointmentService _appointments;
    private readonly ILogger<AppointmentsApiController> _logger;

    public AppointmentsApiController(IPublicAppointmentService appointments, ILogger<AppointmentsApiController> logger)
    {
        _appointments = appointments;
        _logger = logger;
    }

    [HttpGet("AvailableSlots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateOnly date,
        [FromQuery] int departmentId,
        [FromQuery] int? doctorId)
    {
        try
        {
            if (!await _appointments.IsBookableDepartmentAsync(departmentId))
                return BadRequest(new { success = false, message = "Khoa/phòng không hỗ trợ đặt lịch khám trực tuyến" });

            var slots = await _appointments.GetPublicSlotsAsync(date, departmentId, doctorId);
            return Ok(new
            {
                success = true,
                date = date.ToString("yyyy-MM-dd"),
                serverNow = AppointmentSlotRules.GetHospitalNow().ToString("yyyy-MM-ddTHH:mm:ss"),
                slots
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available slots");
            return StatusCode(500, new { success = false, message = "Lỗi máy chủ" });
        }
    }

    [HttpGet("Departments")]
    public async Task<IActionResult> GetDepartments()
    {
        try
        {
            var departments = await _appointments.GetBookableDepartmentsAsync();
            return Ok(new
            {
                success = true,
                departments = departments.Select(d => new { departmentId = d.Id, name = d.Label, kind = 1 })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return StatusCode(500, new { success = false, message = "Lỗi máy chủ" });
        }
    }

    [HttpGet("Doctors")]
    public async Task<IActionResult> GetDoctors([FromQuery] int departmentId)
    {
        try
        {
            if (!await _appointments.IsBookableDepartmentAsync(departmentId))
                return BadRequest(new { success = false, message = "Khoa/phòng không hỗ trợ đặt lịch khám trực tuyến" });

            var doctors = await _appointments.GetDoctorsAsync(departmentId);
            return Ok(new
            {
                success = true,
                doctors = doctors.Select(d => new { staffId = d.Id, fullName = d.Label })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctors");
            return StatusCode(500, new { success = false, message = "Lỗi máy chủ" });
        }
    }

    [HttpPost("Book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var result = await _appointments.BookAsync(request);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message,
            code = result.Code,
            appointmentCode = result.Code,
            appointmentId = result.AppointmentId,
            date = request.Date.ToString("dd/MM/yyyy")
        });
    }

    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelAppointment([FromBody] CancelAppointmentRequest request)
    {
        var result = await _appointments.CancelAsync(request.Code, request.Phone);
        if (!result.Success)
        {
            var status = result.Message == "Không tìm thấy lịch hẹn" ? 404 : 400;
            return StatusCode(status, new { success = false, message = result.Message });
        }

        return Ok(new { success = true, message = result.Message, code = request.Code.Trim().ToUpperInvariant() });
    }

    [HttpGet("Check")]
    public async Task<IActionResult> CheckAppointment([FromQuery] string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { success = false, message = "Mã lịch hẹn không hợp lệ" });

            var appointment = await _appointments.FindByCodeAsync(code, tracking: false);
            if (appointment == null)
                return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });

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
            return StatusCode(500, new { success = false, message = "Lỗi máy chủ" });
        }
    }
}
