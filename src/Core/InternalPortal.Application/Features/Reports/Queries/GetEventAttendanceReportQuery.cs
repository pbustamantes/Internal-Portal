using InternalPortal.Application.Features.Reports.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Reports.Queries;

public record GetEventAttendanceReportQuery : IRequest<IReadOnlyList<EventAttendanceReportDto>>;
