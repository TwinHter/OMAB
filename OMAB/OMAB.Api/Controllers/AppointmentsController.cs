using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Appointments.Commands;
using OMAB.Application.Features.Appointments.DTOs;
using OMAB.Application.Features.Appointments.Queries;
using OMAB.Domain.Entities;
namespace OMAB.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ApiController
{
    [HttpGet("{appointmentId:int}")]
    public async Task<IActionResult> GetAppointmentById([FromRoute] int appointmentId)
    {
        var result = await Sender.Send(new GetAppointmentDetailById.Query(appointmentId));
        return HandleResult(result);
    }

    [HttpGet()]
    public async Task<IActionResult> GetAppointmentsByFilter([FromQuery] GetAppointmentByFilter.Query query)
    {
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointment.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
    [HttpPut("{id:int}/patient-update")]
    public async Task<IActionResult> UpdateAppointmentByPatient([FromRoute] int id, [FromBody] UpdateAppointmentByPatientDto dto)
    {
        var command = new PatientUpdateAppointment.Command(id, dto);
        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    [HttpPut("{id:int}/doctor-update")]
    public async Task<IActionResult> UpdateAppointmentByDoctor([FromRoute] int id, [FromBody] UpdateAppointmentByDoctorDto dto)
    {
        var command = new DoctorUpdateAppointment.Command(id, dto);
        var result = await Sender.Send(command);

        return HandleResult(result);
    }
}
