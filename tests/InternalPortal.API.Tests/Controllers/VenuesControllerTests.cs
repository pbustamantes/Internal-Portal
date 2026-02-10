using Xunit;
using FluentAssertions;
using InternalPortal.API.Controllers;
using InternalPortal.Application.Features.Venues;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InternalPortal.API.Tests.Controllers;

public class VenuesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly VenuesController _controller;

    private static VenueDto CreateVenueDto(Guid? id = null) => new(
        id ?? Guid.NewGuid(), "Main Hall", 200,
        "100 Main St", "Austin", "TX", "78701", "HQ", "Room A");

    public VenuesControllerTests()
    {
        _controller = new VenuesController(_mediator.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithVenues()
    {
        var venues = new List<VenueDto> { CreateVenueDto(), CreateVenueDto() };
        _mediator.Setup(m => m.Send(It.IsAny<GetVenuesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venues);

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(venues);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithVenue()
    {
        var venueId = Guid.NewGuid();
        var venue = CreateVenueDto(venueId);
        _mediator.Setup(m => m.Send(It.IsAny<GetVenueByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        var result = await _controller.GetById(venueId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(venue);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var venue = CreateVenueDto();
        var command = new CreateVenueCommand("Main Hall", 200, "100 Main St", "Austin", "TX", "78701", "HQ", "Room A");
        _mediator.Setup(m => m.Send(It.IsAny<CreateVenueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        var result = await _controller.Create(command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(venue);
        createdResult.ActionName.Should().Be(nameof(VenuesController.GetById));
    }

    [Fact]
    public async Task Update_WithMatchingId_ShouldReturnOk()
    {
        var venueId = Guid.NewGuid();
        var venue = CreateVenueDto(venueId);
        var command = new UpdateVenueCommand(venueId, "Updated Hall", 300, "200 Main St", "Dallas", "TX", "75201", null, null);
        _mediator.Setup(m => m.Send(It.IsAny<UpdateVenueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        var result = await _controller.Update(venueId, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(venue);
    }

    [Fact]
    public async Task Update_WithMismatchedId_ShouldReturnBadRequest()
    {
        var command = new UpdateVenueCommand(Guid.NewGuid(), "Hall", 100, "St", "City", "ST", "00000", null, null);

        var result = await _controller.Update(Guid.NewGuid(), command);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        var venueId = Guid.NewGuid();
        _mediator.Setup(m => m.Send(It.IsAny<DeleteVenueCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var result = await _controller.Delete(venueId);

        result.Should().BeOfType<NoContentResult>();
    }
}
