using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Appointments.Commands;

namespace OMAB.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PrescriptionsController : ApiController
{
    [HttpPut()]
    public async Task<IActionResult> UpdatePrescription([FromBody] UpdatePrescription.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}
