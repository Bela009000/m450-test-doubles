namespace TestDoublesExercise.Tests.TestDoubles;

internal sealed class SeatAvailabilityStub : ISeatAvailability
{
    private readonly Dictionary<string, int> _availableSeats = [];

    public SeatAvailabilityStub(string eventCode, int availableSeats)
    {
        _availableSeats[eventCode] = availableSeats;
    }

    public SeatAvailabilityStub With(string eventCode, int availableSeats)
    {
        _availableSeats[eventCode] = availableSeats;
        return this;
    }

    public int GetAvailableSeats(string eventCode) => _availableSeats.GetValueOrDefault(eventCode);
}
