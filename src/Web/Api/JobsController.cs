using Hippo.Application.Jobs.Commands;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hippo.Web.Api;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class JobsController : ApiControllerBase
{
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartJob([FromRoute] Guid id)
    {
        await Mediator.Send(new StartJobCommand { JobName = id.ToString()});

        return NoContent();
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopJob([FromRoute] Guid id)
    {
        await Mediator.Send(new StopJobCommand { JobName = id.ToString() });

        return NoContent();
    }
}
