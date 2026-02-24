using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMAB.API.Controllers;
using OMAB.Application.Features.Identities;
using OMAB.Application.Features.Identities.Commands;

namespace OMAB.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUser.Command commands)
    {
        var result = await Sender.Send(commands);
        return HandleResult(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUser.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePassword.Command command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var result = await Sender.Send(new LogoutUser.Command());
        return HandleResult(result);
    }
}

