using InternalPortal.Application.Features.Reports.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Reports.Queries;

public record GetMonthlyEventsReportQuery(int Year) : IRequest<IReadOnlyList<MonthlyEventsReportDto>>;
