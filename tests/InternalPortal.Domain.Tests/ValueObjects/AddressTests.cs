using Xunit;
using FluentAssertions;
using InternalPortal.Domain.ValueObjects;

namespace InternalPortal.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreate()
    {
        var address = new Address("123 Main St", "Austin", "TX", "78701", "HQ", "101");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Austin");
        address.Building.Should().Be("HQ");
        address.Room.Should().Be("101");
    }

    [Fact]
    public void Constructor_WithNullStreet_ShouldThrow()
    {
        var act = () => new Address(null!, "Austin", "TX", "78701");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        var addr1 = new Address("123 Main", "Austin", "TX", "78701");
        var addr2 = new Address("123 Main", "Austin", "TX", "78701");

        addr1.Should().Be(addr2);
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        var address = new Address("123 Main St", "Austin", "TX", "78701");
        address.ToString().Should().Be("123 Main St, Austin, TX 78701");
    }
}
