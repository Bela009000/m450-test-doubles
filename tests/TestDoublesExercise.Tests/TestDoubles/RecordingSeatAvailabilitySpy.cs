namespace TestDoublesExercise.Tests.TestDoubles;

internal sealed class RecordingSeatAvailabilitySpy(int availableSeats) : ISeatAvailability
{
    private readonly List<string> _queriedEventCodes = [];

    public IReadOnlyList<string> QueriedEventCodes => _queriedEventCodes;

    public int GetAvailableSeats(string eventCode)
    {
        _queriedEventCodes.Add(eventCode);
        return availableSeats;
    }
}
