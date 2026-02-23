using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Identities.Queries;

namespace OMAB.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ApiController
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var result = await Sender.Send(new GetCurrentUser.Query());
        return HandleResult(result);
    }

    [HttpGet("profile/{userId:int}")]
    public async Task<IActionResult> GetUserProfileById([FromRoute] int userId)
    {
        var result = await Sender.Send(new GetUserById.Query(userId));
        return HandleResult(result);
    }
}

