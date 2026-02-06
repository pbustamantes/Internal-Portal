using Xunit;
using FluentAssertions;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Exceptions;

namespace InternalPortal.Domain.Tests.Entities;

public class RegistrationTests
{
    [Fact]
    public void Confirm_WhenPending_ShouldSetConfirmed()
    {
        var reg = new Registration { Status = RegistrationStatus.Pending };

        reg.Confirm();

        reg.Status.Should().Be(RegistrationStatus.Confirmed);
        reg.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrow()
    {
        var reg = new Registration { Status = RegistrationStatus.Confirmed };
        var act = () => reg.Confirm();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_ShouldSetCancelled()
    {
        var reg = new Registration { Status = RegistrationStatus.Confirmed };

        reg.Cancel();

        reg.Status.Should().Be(RegistrationStatus.Cancelled);
        reg.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var reg = new Registration { Status = RegistrationStatus.Cancelled };
        var act = () => reg.Cancel();

        act.Should().Throw<DomainException>();
    }
}
