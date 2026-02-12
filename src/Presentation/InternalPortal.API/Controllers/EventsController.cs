using InternalPortal.Application.Features.Events.Commands;
using InternalPortal.Application.Features.Events.Queries;
using InternalPortal.Application.Features.Registrations.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = null)
    {
        var result = await _mediator.Send(new GetEventsQuery(page, pageSize, search, categoryId, sortBy, sortOrder));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEvent(Guid id)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await _mediator.Send(new DeleteEventCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        await _mediator.Send(new PublishEventCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelEvent(Guid id)
    {
        await _mediator.Send(new CancelEventCommand(id));
        return NoContent();
    }

    [HttpGet("{id:guid}/attendees")]
    public async Task<IActionResult> GetAttendees(Guid id)
    {
        var result = await _mediator.Send(new GetEventAttendeesQuery(id));
        return Ok(result);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int count = 5)
    {
        var result = await _mediator.Send(new GetUpcomingEventsQuery(count));
        return Ok(result);
    }

    [HttpPost("{eventId:guid}/register")]
    public async Task<IActionResult> Register(Guid eventId)
    {
        var result = await _mediator.Send(new CreateRegistrationCommand(eventId));
        return Ok(result);
    }

    [HttpDelete("{eventId:guid}/register")]
    public async Task<IActionResult> CancelRegistration(Guid eventId)
    {
        await _mediator.Send(new CancelRegistrationCommand(eventId));
        return NoContent();
    }
}
