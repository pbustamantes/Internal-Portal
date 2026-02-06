using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Reports.DTOs;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace InternalPortal.Application.Features.Reports.Queries;

public class GetMonthlyEventsReportQueryHandler : IRequestHandler<GetMonthlyEventsReportQuery, IReadOnlyList<MonthlyEventsReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyEventsReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MonthlyEventsReportDto>> Handle(GetMonthlyEventsReportQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Include(e => e.Registrations)
            .Where(e => e.Schedule.StartUtc.Year == request.Year)
            .ToListAsync(cancellationToken);

        return events
            .GroupBy(e => e.Schedule.StartUtc.Month)
            .Select(g => new MonthlyEventsReportDto(
                request.Year,
                g.Key,
                g.Count(),
                g.Sum(e => e.Registrations.Count),
                g.Count() > 0 ? g.Sum(e => e.Registrations.Count) / g.Count() : 0))
            .OrderBy(r => r.Month)
            .ToList();
    }
}
