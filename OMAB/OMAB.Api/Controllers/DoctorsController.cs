using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Appointments.Queries;
using OMAB.Application.Features.Identities.Queries;
using OMAB.Application.Features.Profiles.Commands;
using OMAB.Application.Features.Profiles.Queries;

namespace OMAB.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ApiController
{
    [HttpPost("filter")]
    public async Task<IActionResult> GetDoctorsByFilter([FromBody] GetDoctorsByFilter.Query query)
    {
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableDoctorSlots([FromQuery] GetDoctorAvailableSlot.Query query)
    {
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateDoctorProfile([FromBody] UpdateDoctorProfile.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpPut("specialties")]
    public async Task<IActionResult> UpdateDoctorSpecialty([FromBody] UpdateDoctorSpecialty.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpGet("schedules/{doctorId:int?}")]
    public async Task<IActionResult> GetDoctorSchedules([FromRoute] int? doctorId = null)
    {
        var query = new GetDoctorSchedules.Query(doctorId);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }
    [HttpPost("schedules")]
    public async Task<IActionResult> CreateDoctorSchedule([FromBody] CreateDoctorSchedule.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("schedules/{scheduleId:int}")]
    public async Task<IActionResult> DeleteDoctorSchedule([FromRoute] int scheduleId)
    {
        var result = await Sender.Send(new DeleteDoctorSchedule.Command(scheduleId));
        return HandleResult(result);
    }

    [HttpGet("{doctorId:int}/reviews")]
    public async Task<IActionResult> GetDoctorReviews([FromRoute] int doctorId)
    {
        var result = await Sender.Send(new GetReviewByDoctorId.Query(doctorId));
        return HandleResult(result);
    }
}
