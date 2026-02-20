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
    [HttpGet("profile")]
    public async Task<IActionResult> GetCurrentPatientProfile()
    {
        var result = await Sender.Send(new GetCurrentUser.Query());
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPatientById([FromRoute] int id)
    {
        var result = await Sender.Send(new GetUserById.Query(id));
        return HandleResult(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdatePatientProfile([FromBody] UpdatePatientProfile.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}

