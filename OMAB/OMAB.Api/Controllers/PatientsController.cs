using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Identities.Queries;
using OMAB.Application.Features.Profiles.Commands;

namespace OMAB.Api.Controllers;

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

