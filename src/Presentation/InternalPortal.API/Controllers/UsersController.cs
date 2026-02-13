using InternalPortal.Application.Features.Registrations.Queries;
using InternalPortal.Application.Features.Users.Commands;
using InternalPortal.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery());
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("me/profile-picture")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new { detail = "File is empty." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { detail = "File size exceeds 5 MB limit." });

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { detail = "Only jpg, png, gif, and webp files are allowed." });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UploadProfilePictureCommand(extension, stream));
        return Ok(result);
    }

    [HttpDelete("me/profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        var result = await _mediator.Send(new DeleteProfilePictureCommand());
        return Ok(result);
    }

    [HttpGet("me/events")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var result = await _mediator.Send(new GetUserRegistrationsQuery());
        return Ok(result);
    }
}
