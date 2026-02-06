using Xunit;
using FluentAssertions;
using InternalPortal.Domain.ValueObjects;

namespace InternalPortal.Domain.Tests.ValueObjects;

public class CapacityTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreate()
    {
        var capacity = new Capacity(5, 100);

        capacity.MinAttendees.Should().Be(5);
        capacity.MaxAttendees.Should().Be(100);
    }

    [Fact]
    public void Constructor_WithNegativeMin_ShouldThrow()
    {
        var act = () => new Capacity(-1, 100);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithMaxLessThanMin_ShouldThrow()
    {
        var act = () => new Capacity(10, 5);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsFull_WhenAtCapacity_ShouldReturnTrue()
    {
        var capacity = new Capacity(0, 10);
        capacity.IsFull(10).Should().BeTrue();
    }

    [Fact]
    public void IsFull_WhenBelowCapacity_ShouldReturnFalse()
    {
        var capacity = new Capacity(0, 10);
        capacity.IsFull(5).Should().BeFalse();
    }

    [Fact]
    public void RemainingSpots_ShouldReturnCorrectCount()
    {
        var capacity = new Capacity(0, 10);
        capacity.RemainingSpots(7).Should().Be(3);
    }
}
