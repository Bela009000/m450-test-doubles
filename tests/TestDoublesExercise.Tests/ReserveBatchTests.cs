using Moq;
using TestDoublesExercise.Tests.TestDoubles;

namespace TestDoublesExercise.Tests;

public sealed class ReserveBatchTests
{
    private const string Concert = "KONZERT-1";
    private const string Theatre = "THEATER-7";

    [Fact]
    public void ReserveBatch_WithNullRequests_ThrowsBeforeCallingDependencies()
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var exception = Assert.Throws<ArgumentNullException>(() => service.ReserveBatch(null!));

        Assert.Equal("requests", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WithEmptyRequests_ThrowsBeforeCallingDependencies()
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        var exception = Assert.Throws<ArgumentException>(() => service.ReserveBatch([]));

        Assert.Equal("requests", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(sender.SentReservations);
    }

    [Theory]
    [InlineData("", Concert)]
    [InlineData("   ", Concert)]
    [InlineData("ben@example.com", "")]
    [InlineData("ben@example.com", "   ")]
    public void ReserveBatch_WithBlankEmailOrEventCodeInAnyEntry_ThrowsBeforeCallingDependencies(
        string customerEmail, string eventCode)
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 1),
            new(customerEmail, eventCode, 1),
        ];

        var exception = Assert.Throws<ArgumentException>(() => service.ReserveBatch(requests));

        Assert.Equal("requests", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(sender.SentReservations);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReserveBatch_WithNonPositiveSeatCountInAnyEntry_ThrowsBeforeCallingDependencies(int seatCount)
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 1),
            new("ben@example.com", Concert, seatCount),
        ];

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => service.ReserveBatch(requests));

        Assert.Equal("requests", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WithNullEntry_ThrowsBeforeCallingDependencies()
    {
        var availability = new RecordingSeatAvailabilitySpy(availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);
        Reservation[] requests = [new("anna@example.com", Concert, 1), null!];

        var exception = Assert.Throws<ArgumentException>(() => service.ReserveBatch(requests));

        Assert.Equal("requests", exception.ParamName);
        Assert.Empty(availability.QueriedEventCodes);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WithSeveralRequestsPerEvent_QueriesAvailabilityOncePerEvent()
    {
        var availability = new Mock<ISeatAvailability>();
        availability.Setup(a => a.GetAvailableSeats(It.IsAny<string>())).Returns(10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability.Object, repository, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Theatre, 1),
            new("cleo@example.com", Concert, 3),
        ];

        var result = service.ReserveBatch(requests);

        Assert.True(result);
        availability.Verify(a => a.GetAvailableSeats(Concert), Times.Once());
        availability.Verify(a => a.GetAvailableSeats(Theatre), Times.Once());
        availability.VerifyNoOtherCalls();
    }

    [Fact]
    public void ReserveBatch_WhenCombinedSeatsExceedCapacity_DoesNotSaveOrSend()
    {
        var availability = new SeatAvailabilityStub(Concert, availableSeats: 3);
        var repository = new Mock<IReservationRepository>();
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Concert, 2),
        ];

        var result = service.ReserveBatch(requests);

        Assert.False(result);
        repository.Verify(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>()), Times.Never());
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WhenCombinedSeatsExactlyMatchCapacity_Succeeds()
    {
        var availability = new SeatAvailabilityStub(Concert, availableSeats: 3);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Concert, 1),
        ];

        var result = service.ReserveBatch(requests);

        Assert.True(result);
        Assert.Single(repository.SavedBatches);
    }

    [Fact]
    public void ReserveBatch_WhenOneOfSeveralEventsHasTooFewSeats_DoesNotSaveOrSend()
    {
        var availability = new SeatAvailabilityStub(Concert, availableSeats: 10).With(Theatre, availableSeats: 1);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Theatre, 2),
        ];

        var result = service.ReserveBatch(requests);

        Assert.False(result);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WithEnoughSeats_SavesAllReservationsInOneBatchCall()
    {
        var availability = new SeatAvailabilityStub(Concert, availableSeats: 10).With(Theatre, availableSeats: 10);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>())).Returns(true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Theatre, 1),
            new("cleo@example.com", Concert, 3),
        ];

        var result = service.ReserveBatch(requests);

        Assert.True(result);
        repository.Verify(
            r => r.TrySaveBatch(It.Is<IReadOnlyList<Reservation>>(saved =>
                saved.Count == 3 &&
                saved[0].CustomerEmail == "anna@example.com" && saved[0].EventCode == Concert && saved[0].SeatCount == 2 &&
                saved[1].CustomerEmail == "ben@example.com" && saved[1].EventCode == Theatre && saved[1].SeatCount == 1 &&
                saved[2].CustomerEmail == "cleo@example.com" && saved[2].EventCode == Concert && saved[2].SeatCount == 3)),
            Times.Once());
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void ReserveBatch_WhenBatchSaveFails_ReturnsFalseAndSendsNoConfirmation()
    {
        var availability = new SeatAvailabilityStub(Concert, availableSeats: 10);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>())).Returns(false);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Concert, 1),
        ];

        var result = service.ReserveBatch(requests);

        Assert.False(result);
        repository.Verify(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>()), Times.Once());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WhenSaved_SendsOneConfirmationPerRequestInInputOrder()
    {
        var availability = new SeatAvailabilityStub(Concert, availableSeats: 10).With(Theatre, availableSeats: 10);
        var repository = new RecordingReservationRepositorySpy(saveSucceeds: true);
        var sender = new RecordingConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Theatre, 1),
            new("cleo@example.com", Concert, 3),
        ];

        var result = service.ReserveBatch(requests);

        Assert.True(result);
        Assert.Equal(requests, sender.SentReservations);
    }
}
