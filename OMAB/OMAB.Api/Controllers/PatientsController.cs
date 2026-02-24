using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Profiles.Commands;

namespace OMAB.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PatientsController : ApiController
{
    [HttpPut("profile")]
    public async Task<IActionResult> UpdatePatientProfile([FromBody] UpdatePatientProfile.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}

