using InternalPortal.Application.Features.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;

    public CalendarController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await _mediator.Send(new GetEventsByDateRangeQuery(start, end));
        return Ok(result);
    }
}
