using InternalPortal.Application.Common.Models;
using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PaginatedList<EventSummaryDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetEventsQueryHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<EventSummaryDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _eventRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.CategoryId, cancellationToken);

        var pastPublished = items.Where(e => e.Status == EventStatus.Published && e.IsInPast).ToList();
        if (pastPublished.Count > 0)
        {
            foreach (var e in pastPublished)
                e.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var dtos = items.Select(e => new EventSummaryDto(
            e.Id, e.Title,
            e.Schedule.StartUtc, e.Schedule.EndUtc,
            e.Capacity.MaxAttendees,
            e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed),
            e.Status.ToString(),
            e.Category?.Name, e.Category?.ColorHex,
            e.Organizer?.FullName ?? "")).ToList();

        return new PaginatedList<EventSummaryDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
