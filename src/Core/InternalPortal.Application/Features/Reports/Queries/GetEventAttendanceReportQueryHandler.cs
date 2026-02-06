using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Reports.DTOs;
using InternalPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace InternalPortal.Application.Features.Reports.Queries;

public class GetEventAttendanceReportQueryHandler : IRequestHandler<GetEventAttendanceReportQuery, IReadOnlyList<EventAttendanceReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventAttendanceReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EventAttendanceReportDto>> Handle(GetEventAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Include(e => e.Registrations)
            .Where(e => e.Status != EventStatus.Draft)
            .ToListAsync(cancellationToken);

        return events.Select(e =>
        {
            var total = e.Registrations.Count;
            var confirmed = e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
            var cancelled = e.Registrations.Count(r => r.Status == RegistrationStatus.Cancelled);
            var waitlisted = e.Registrations.Count(r => r.Status == RegistrationStatus.Waitlisted);
            var rate = e.Capacity.MaxAttendees > 0 ? (double)confirmed / e.Capacity.MaxAttendees * 100 : 0;

            return new EventAttendanceReportDto(
                e.Id, e.Title, e.Schedule.StartUtc,
                total, confirmed, cancelled, waitlisted, Math.Round(rate, 1));
        }).ToList();
    }
}
