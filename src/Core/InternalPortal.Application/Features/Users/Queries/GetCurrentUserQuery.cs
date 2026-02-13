using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Users.Queries;

public record GetCurrentUserQuery : IRequest<UserDto>;
