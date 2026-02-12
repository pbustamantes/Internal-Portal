using InternalPortal.Application.Common.Models;
using InternalPortal.Application.Features.Events.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public record GetEventsQuery(int Page = 1, int PageSize = 10, string? Search = null, Guid? CategoryId = null, string? SortBy = null, string? SortOrder = null) : IRequest<PaginatedList<EventSummaryDto>>;
