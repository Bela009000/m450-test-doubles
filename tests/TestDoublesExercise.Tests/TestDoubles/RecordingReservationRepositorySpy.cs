namespace TestDoublesExercise.Tests.TestDoubles;

internal sealed class RecordingReservationRepositorySpy(bool saveSucceeds) : IReservationRepository
{
    private readonly List<Reservation> _savedReservations = [];
    private readonly List<IReadOnlyList<Reservation>> _savedBatches = [];

    public IReadOnlyList<Reservation> SavedReservations => _savedReservations;

    public IReadOnlyList<IReadOnlyList<Reservation>> SavedBatches => _savedBatches;

    public bool TrySave(Reservation reservation)
    {
        _savedReservations.Add(reservation);
        return saveSucceeds;
    }

    public bool TrySaveBatch(IReadOnlyList<Reservation> reservations)
    {
        _savedBatches.Add([.. reservations]);
        return saveSucceeds;
    }
}
