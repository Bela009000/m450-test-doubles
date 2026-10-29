using Moq;
using TestDoublesExercise.Tests.TestDoubles;

namespace TestDoublesExercise.Tests;

public sealed class ReservationServiceTests
{
    private const string Email = "anna@example.com";
    private const string EventCode = "KONZERT-1";

    [Fact]
    public void Reserve_WithEnoughSeats_ReturnsTrue()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 3);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.True(result);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotSaveOrSend()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 1);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.False(result);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithExactlyEnoughSeats_Succeeds()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 2);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.True(result);
    }

    [Fact]
    public void Reserve_WithZeroSeats_ThrowsBeforeCallingDependencies()
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Reserve(Email, EventCode, seatCount: 0));

        Assert.Equal("seatCount", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithNegativeSeats_ThrowsBeforeCallingDependencies()
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Reserve(Email, EventCode, seatCount: -1));

        Assert.Equal("seatCount", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Reserve_WithEmptyEventCode_ThrowsBeforeCallingDependencies(string eventCode)
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var exception = Assert.Throws<ArgumentException>(
            () => service.Reserve(Email, eventCode, seatCount: 2));

        Assert.Equal("eventCode", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WhenSaved_SendsExactlyOneConfirmationWithReservationData()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 3);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        service.Reserve(Email, EventCode, seatCount: 2);

        var confirmation = Assert.Single(sender.SentReservations);
        Assert.Equal(Email, confirmation.CustomerEmail);
        Assert.Equal(EventCode, confirmation.EventCode);
        Assert.Equal(2, confirmation.SeatCount);
    }

    [Fact]
    public void Reserve_WhenSavingFails_DoesNotSendConfirmation()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 3);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: false);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.False(result);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithEnoughSeats_SavesCorrectReservationOnce()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 3);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.True(result);
        repository.Verify(
            r => r.TrySave(It.Is<Reservation>(reservation =>
                reservation.CustomerEmail == Email &&
                reservation.EventCode == EventCode &&
                reservation.SeatCount == 2)),
            Times.Once());
        Assert.Single(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WhenMockedSaveFails_ReturnsFalseAndDoesNotSend()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 3);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(false);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.False(result);
        repository.Verify(
            r => r.TrySave(It.Is<Reservation>(reservation =>
                reservation.CustomerEmail == Email &&
                reservation.EventCode == EventCode &&
                reservation.SeatCount == 2)),
            Times.Once());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotCallRepository()
    {
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 1);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);

        var result = service.Reserve(Email, EventCode, seatCount: 2);

        Assert.False(result);
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithWhitespaceEmail_ThrowsBeforeCallingDependencies()
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new Mock<IReservationRepository>();
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);

        var exception = Assert.Throws<ArgumentException>(
            () => service.Reserve("   ", EventCode, seatCount: 2));

        Assert.Equal("customerEmail", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Empty(sender.SentReservations);
    }
}
