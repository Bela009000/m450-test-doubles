using System.Globalization;

namespace M450.Bankkonto;

/// <summary>
/// Startpunkt für eine beispielhafte Ausführung des Bankkontos.
/// </summary>
public static class Bank
{
    public static void Main()
    {
        var konto = new Bankkonto(
            aktivZins: 3.6m,
            passivZins: 7.2m);

        Console.WriteLine("Bankkonto-Jahresbeispiel");
        Console.WriteLine("Aktivzins: 3,6 %, Passivzins: 7,2 %\n");

        Console.WriteLine("1. März: Konto eröffnen und CHF 1'000 einzahlen");
        konto.ZahleEin(1_000m);
        ZeigeKonto(konto);

        Console.WriteLine("\n1. Juli: Zins für 120 Tage und CHF 1'000 einzahlen");
        konto.SchreibeZinsGut(120);
        konto.ZahleEin(1_000m);
        ZeigeKonto(konto);

        Console.WriteLine("\n1. August: Zins für 30 Tage und CHF 3'000 beziehen");
        konto.SchreibeZinsGut(30);
        konto.Beziehe(3_000m);
        ZeigeKonto(konto);

        Console.WriteLine("\n1. Oktober: Passivzins für 60 Tage und CHF 2'000 einzahlen");
        konto.SchreibeZinsGut(60);
        konto.ZahleEin(2_000m);
        ZeigeKonto(konto);

        Console.WriteLine("\n31. Dezember: Zins für 90 Tage berechnen");
        konto.SchreibeZinsGut(90);
        ZeigeKonto(konto);

        Console.WriteLine("\n31. Dezember: Konto abschliessen und Zins verbuchen");
        konto.SchliesseKontoAb();
        ZeigeKonto(konto);
    }

    private static void ZeigeKonto(Bankkonto konto)
    {
        var kultur = CultureInfo.GetCultureInfo("de-CH");
        var guthaben = konto.Guthaben.ToString("C2", kultur);
        var zins = konto.AufgelaufenerZins.ToString("C2", kultur);

        Console.WriteLine(
            $"Konto {konto.KontoNummer}: Guthaben {guthaben}, "
            + $"aufgelaufener Zins {zins}");
    }
}
