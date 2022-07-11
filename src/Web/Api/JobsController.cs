using Hippo.Application.Jobs.Commands;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hippo.Web.Api;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class JobsController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> StopJob([FromBody] StopJobCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> StartJob([FromBody] StartJobCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }
}