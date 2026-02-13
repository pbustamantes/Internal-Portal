using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Users.Commands;

public record UploadProfilePictureCommand(string Extension, Stream FileStream) : IRequest<UserDto>;
