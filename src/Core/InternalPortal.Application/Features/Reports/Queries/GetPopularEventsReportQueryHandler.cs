using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Reports.DTOs;
using InternalPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace InternalPortal.Application.Features.Reports.Queries;

public class GetPopularEventsReportQueryHandler : IRequestHandler<GetPopularEventsReportQuery, IReadOnlyList<PopularEventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPopularEventsReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PopularEventDto>> Handle(GetPopularEventsReportQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Include(e => e.Registrations)
            .Include(e => e.Category)
            .Where(e => e.Status == EventStatus.Published || e.Status == EventStatus.Completed)
            .ToListAsync(cancellationToken);

        return events
            .Select(e =>
            {
                var regCount = e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
                var fillRate = e.Capacity.MaxAttendees > 0 ? (double)regCount / e.Capacity.MaxAttendees * 100 : 0;
                return new PopularEventDto(e.Id, e.Title, e.Category?.Name, regCount, e.Capacity.MaxAttendees, Math.Round(fillRate, 1));
            })
            .OrderByDescending(e => e.RegistrationCount)
            .Take(request.Top)
            .ToList();
    }
}
