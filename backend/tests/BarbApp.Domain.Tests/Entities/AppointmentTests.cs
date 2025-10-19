using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Exceptions;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class AppointmentTests
{
    private readonly Guid _barbeariaId = Guid.NewGuid();
    private readonly Guid _barberId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly DateTime _startTime = DateTime.UtcNow.AddHours(1);
    private readonly DateTime _endTime = DateTime.UtcNow.AddHours(2);

    [Fact]
    public void Create_ValidParameters_ShouldSucceed()
    {
        // Act
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        // Assert
        appointment.Should().NotBeNull();
        appointment.Id.Should().NotBe(Guid.Empty);
        appointment.BarbeariaId.Should().Be(_barbeariaId);
        appointment.BarberId.Should().Be(_barberId);
        appointment.CustomerId.Should().Be(_customerId);
        appointment.ServiceId.Should().Be(_serviceId);
        appointment.StartTime.Should().Be(_startTime);
        appointment.EndTime.Should().Be(_endTime);
        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.ConfirmedAt.Should().BeNull();
        appointment.CancelledAt.Should().BeNull();
        appointment.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyBarbeariaId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Appointment.Create(
            Guid.Empty,
            _barberId,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("BarbeariaId is required*");
    }

    [Fact]
    public void Create_EmptyBarberId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Appointment.Create(
            _barbeariaId,
            Guid.Empty,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("BarberId is required*");
    }

    [Fact]
    public void Create_EmptyCustomerId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Appointment.Create(
            _barbeariaId,
            _barberId,
            Guid.Empty,
            _serviceId,
            _startTime,
            _endTime);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CustomerId is required*");
    }

    [Fact]
    public void Create_EmptyServiceId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            Guid.Empty,
            _startTime,
            _endTime);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ServiceId is required*");
    }

    [Fact]
    public void Create_EndTimeBeforeStartTime_ShouldThrowArgumentException()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(2);
        var endTime = DateTime.UtcNow.AddHours(1);

        // Act
        var act = () => Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            startTime,
            endTime);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("EndTime must be after StartTime*");
    }

    [Fact]
    public void Confirm_PendingAppointment_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.ConfirmedAt.Should().NotBeNull();
        appointment.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    public void Confirm_NonPendingAppointment_ShouldThrowException(AppointmentStatus status)
    {
        // Arrange
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        // Set status using reflection or create helper
        SetAppointmentStatus(appointment, status);

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidAppointmentStatusTransitionException>()
            .WithMessage($"Cannot confirm appointment with status '{status}'");
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Confirmed)]
    public void Cancel_PendingOrConfirmedAppointment_ShouldUpdateStatusAndTimestamp(AppointmentStatus status)
    {
        // Arrange
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        SetAppointmentStatus(appointment, status);

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledAt.Should().NotBeNull();
        appointment.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    public void Cancel_CompletedOrCancelledAppointment_ShouldThrowException(AppointmentStatus status)
    {
        // Arrange
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            _startTime,
            _endTime);

        SetAppointmentStatus(appointment, status);

        // Act
        var act = () => appointment.Cancel();

        // Assert
        act.Should().Throw<InvalidAppointmentStatusTransitionException>()
            .WithMessage($"Cannot cancel appointment with status '{status}'");
    }

    [Fact]
    public void Complete_ConfirmedAppointmentAfterStartTime_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-1); // In the past
        var endTime = DateTime.UtcNow.AddHours(1);
        
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            startTime,
            endTime);

        appointment.Confirm();

        // Act
        appointment.Complete();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.CompletedAt.Should().NotBeNull();
        appointment.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    public void Complete_NonConfirmedAppointment_ShouldThrowException(AppointmentStatus status)
    {
        // Arrange
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));

        SetAppointmentStatus(appointment, status);

        // Act
        var act = () => appointment.Complete();

        // Assert
        act.Should().Throw<InvalidAppointmentStatusTransitionException>()
            .WithMessage($"Cannot complete appointment with status '{status}'");
    }

    [Fact]
    public void Complete_BeforeStartTime_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(2); // In the future
        var endTime = DateTime.UtcNow.AddHours(3);
        
        var appointment = Appointment.Create(
            _barbeariaId,
            _barberId,
            _customerId,
            _serviceId,
            startTime,
            endTime);

        appointment.Confirm();

        // Act
        var act = () => appointment.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot complete an appointment that has not started yet.");
    }

    // Helper method to set status using reflection
    private void SetAppointmentStatus(Appointment appointment, AppointmentStatus status)
    {
        var statusProperty = typeof(Appointment).GetProperty("Status");
        statusProperty!.SetValue(appointment, status);

        // Also set the appropriate timestamp based on status
        var now = DateTime.UtcNow;
        switch (status)
        {
            case AppointmentStatus.Confirmed:
                var confirmedAtProperty = typeof(Appointment).GetProperty("ConfirmedAt");
                confirmedAtProperty!.SetValue(appointment, now);
                break;
            case AppointmentStatus.Cancelled:
                var cancelledAtProperty = typeof(Appointment).GetProperty("CancelledAt");
                cancelledAtProperty!.SetValue(appointment, now);
                break;
            case AppointmentStatus.Completed:
                var completedAtProperty = typeof(Appointment).GetProperty("CompletedAt");
                completedAtProperty!.SetValue(appointment, now);
                break;
        }
    }
}
