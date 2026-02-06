using InternalPortal.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport()
    {
        var result = await _mediator.Send(new GetEventAttendanceReportQuery());
        return Ok(result);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int? year = null)
    {
        var result = await _mediator.Send(new GetMonthlyEventsReportQuery(year ?? DateTime.UtcNow.Year));
        return Ok(result);
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularEvents([FromQuery] int top = 10)
    {
        var result = await _mediator.Send(new GetPopularEventsReportQuery(top));
        return Ok(result);
    }
}
