namespace M450.Bankkonto;

/// <summary>
/// Repräsentiert ein Bankkonto mit einfacher, täglich berechneter Verzinsung.
/// Die Zinssätze werden als Prozentwerte angegeben (zum Beispiel 1.5 für 1,5 %).
/// </summary>
public sealed class Bankkonto
{
    private const int BankjahrInTagen = 360;
    private static long _letzteKontoNummer = 100_000;

    /// <summary>
    /// Erstellt ein neues Konto mit einer automatisch vergebenen Kontonummer.
    /// </summary>
    /// <param name="aktivZins">Jährlicher Habenzins in Prozent.</param>
    /// <param name="passivZins">Jährlicher Sollzins in Prozent.</param>
    /// <param name="startGuthaben">Anfänglicher Kontostand.</param>
    public Bankkonto(
        decimal aktivZins,
        decimal passivZins,
        decimal startGuthaben = 0m)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(aktivZins);
        ArgumentOutOfRangeException.ThrowIfNegative(passivZins);

        KontoNummer = Interlocked.Increment(ref _letzteKontoNummer);
        AktivZins = aktivZins;
        PassivZins = passivZins;
        Guthaben = startGuthaben;
    }

    /// <summary>
    /// Eindeutige, nach dem Erstellen nicht mehr veränderbare Kontonummer.
    /// </summary>
    public long KontoNummer { get; }

    /// <summary>
    /// Aktueller Kontostand. Er kann nur durch Kontooperationen verändert werden.
    /// </summary>
    public decimal Guthaben { get; private set; }

    /// <summary>
    /// Jährlicher Habenzins in Prozent.
    /// </summary>
    public decimal AktivZins { get; }

    /// <summary>
    /// Jährlicher Sollzins in Prozent.
    /// </summary>
    public decimal PassivZins { get; }

    /// <summary>
    /// Seit dem letzten Abschluss aufgelaufener, noch nicht verbuchter Zins.
    /// </summary>
    public decimal AufgelaufenerZins { get; private set; }

    /// <summary>
    /// Zahlt einen positiven Betrag auf das Konto ein.
    /// </summary>
    public void ZahleEin(decimal betrag)
    {
        StellePositivenBetragSicher(betrag);
        Guthaben += betrag;
    }

    /// <summary>
    /// Bezieht einen positiven Betrag. Eine Überziehung ist zulässig.
    /// </summary>
    public void Beziehe(decimal betrag)
    {
        StellePositivenBetragSicher(betrag);
        Guthaben -= betrag;
    }

    /// <summary>
    /// Überweist einen positiven Betrag auf ein anderes Konto.
    /// </summary>
    public void Transferiere(Bankkonto konto, decimal betrag)
    {
        ArgumentNullException.ThrowIfNull(konto);
        StellePositivenBetragSicher(betrag);

        if (ReferenceEquals(this, konto))
        {
            throw new ArgumentException(
                "Quell- und Zielkonto müssen verschieden sein.",
                nameof(konto));
        }

        Beziehe(betrag);
        konto.ZahleEin(betrag);
    }

    /// <summary>
    /// Berechnet den Zins für die angegebene Anzahl Tage nach der 360-Tage-Methode.
    /// Der Zins wird separat gesammelt und noch nicht dem Guthaben zugeschlagen.
    /// </summary>
    public void SchreibeZinsGut(int anzTage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(anzTage);

        var zinsSatz = Guthaben >= 0m ? AktivZins : PassivZins;
        AufgelaufenerZins += Guthaben * zinsSatz * anzTage
            / (100m * BankjahrInTagen);
    }

    /// <summary>
    /// Verbucht den aufgelaufenen Zins und beginnt eine neue Zinsperiode.
    /// </summary>
    public void SchliesseKontoAb()
    {
        Guthaben += AufgelaufenerZins;
        AufgelaufenerZins = 0m;
    }

    private static void StellePositivenBetragSicher(decimal betrag)
    {
        if (betrag <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(betrag),
                "Der Betrag muss grösser als null sein.");
        }
    }
}
