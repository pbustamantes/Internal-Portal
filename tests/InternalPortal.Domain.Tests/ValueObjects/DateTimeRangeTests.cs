using Xunit;
using FluentAssertions;
using InternalPortal.Domain.ValueObjects;

namespace InternalPortal.Domain.Tests.ValueObjects;

public class DateTimeRangeTests
{
    [Fact]
    public void Constructor_WithValidRange_ShouldCreate()
    {
        var start = DateTime.UtcNow;
        var end = start.AddHours(2);

        var range = new DateTimeRange(start, end);

        range.StartUtc.Should().Be(start);
        range.EndUtc.Should().Be(end);
    }

    [Fact]
    public void Constructor_WithEndBeforeStart_ShouldThrow()
    {
        var start = DateTime.UtcNow;
        var end = start.AddHours(-1);

        var act = () => new DateTimeRange(start, end);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Overlaps_WithOverlappingRanges_ShouldReturnTrue()
    {
        var range1 = new DateTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(2));
        var range2 = new DateTimeRange(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));

        range1.Overlaps(range2).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithNonOverlappingRanges_ShouldReturnFalse()
    {
        var range1 = new DateTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var range2 = new DateTimeRange(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(3));

        range1.Overlaps(range2).Should().BeFalse();
    }

    [Fact]
    public void Duration_ShouldReturnCorrectTimeSpan()
    {
        var start = DateTime.UtcNow;
        var range = new DateTimeRange(start, start.AddHours(3));

        range.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        var start = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var range1 = new DateTimeRange(start, end);
        var range2 = new DateTimeRange(start, end);

        range1.Should().Be(range2);
    }
}
