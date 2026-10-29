namespace TestDoublesExercise;

public sealed record Reservation(string CustomerEmail, string EventCode, int SeatCount);

public interface ISeatAvailability
{
    int GetAvailableSeats(string eventCode);
}

public interface IReservationRepository
{
    bool TrySave(Reservation reservation);

    bool TrySaveBatch(IReadOnlyList<Reservation> reservations);
}

public interface IConfirmationSender
{
    void Send(Reservation reservation);
}

public sealed class ReservationService
{
    private readonly ISeatAvailability _availability;
    private readonly IReservationRepository _repository;
    private readonly IConfirmationSender _sender;

    public ReservationService(
        ISeatAvailability availability,
        IReservationRepository repository,
        IConfirmationSender sender)
    {
        _availability = availability ?? throw new ArgumentNullException(nameof(availability));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public bool Reserve(string customerEmail, string eventCode, int seatCount)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new ArgumentException("Eine E-Mail-Adresse ist erforderlich.", nameof(customerEmail));
        }

        if (string.IsNullOrWhiteSpace(eventCode))
        {
            throw new ArgumentException("Ein Veranstaltungscode ist erforderlich.", nameof(eventCode));
        }

        if (seatCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seatCount));
        }

        if (_availability.GetAvailableSeats(eventCode) < seatCount)
        {
            return false;
        }

        var reservation = new Reservation(customerEmail, eventCode, seatCount);
        if (!_repository.TrySave(reservation))
        {
            return false;
        }

        _sender.Send(reservation);
        return true;
    }

    public bool ReserveBatch(IReadOnlyList<Reservation> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        if (requests.Count == 0)
        {
            throw new ArgumentException("Mindestens eine Reservierung ist erforderlich.", nameof(requests));
        }

        foreach (var request in requests)
        {
            if (request is null)
            {
                throw new ArgumentException("Die Liste darf keine leeren Einträge enthalten.", nameof(requests));
            }

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            {
                throw new ArgumentException("Jede Reservierung braucht eine E-Mail-Adresse.", nameof(requests));
            }

            if (string.IsNullOrWhiteSpace(request.EventCode))
            {
                throw new ArgumentException("Jede Reservierung braucht einen Veranstaltungscode.", nameof(requests));
            }

            if (request.SeatCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requests), "Jede Reservierung braucht mindestens einen Platz.");
            }
        }

        var seatsPerEvent = requests
            .GroupBy(request => request.EventCode)
            .Select(group => new
            {
                Requested = group.Sum(request => request.SeatCount),
                Available = _availability.GetAvailableSeats(group.Key),
            })
            .ToList();

        if (seatsPerEvent.Any(seats => seats.Available < seats.Requested))
        {
            return false;
        }

        if (!_repository.TrySaveBatch(requests))
        {
            return false;
        }

        foreach (var reservation in requests)
        {
            _sender.Send(reservation);
        }

        return true;
    }
}
