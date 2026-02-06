using Xunit;
using FluentAssertions;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;

namespace InternalPortal.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ShouldReturnUserWithCorrectProperties()
    {
        var user = User.Create("test@example.com", "hash", "John", "Doe", "Engineering");

        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Department.Should().Be("Engineering");
        user.Role.Should().Be(UserRole.Employee);
        user.IsActive.Should().BeTrue();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithOrganizerRole_ShouldSetCorrectRole()
    {
        var user = User.Create("test@example.com", "hash", "Jane", "Doe", role: UserRole.Organizer);
        user.Role.Should().Be(UserRole.Organizer);
    }

    [Fact]
    public void FullName_ShouldReturnCombinedName()
    {
        var user = User.Create("test@example.com", "hash", "John", "Doe");
        user.FullName.Should().Be("John Doe");
    }
}
