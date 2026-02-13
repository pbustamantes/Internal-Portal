using System.Text.Json.Serialization;

namespace InternalPortal.Application.Features.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    [property: JsonIgnore] string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Department,
    string Role,
    string? ProfilePictureUrl = null);
