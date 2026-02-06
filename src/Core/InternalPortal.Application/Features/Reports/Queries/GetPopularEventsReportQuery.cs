using InternalPortal.Application.Features.Reports.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Reports.Queries;

public record GetPopularEventsReportQuery(int Top = 10) : IRequest<IReadOnlyList<PopularEventDto>>;
