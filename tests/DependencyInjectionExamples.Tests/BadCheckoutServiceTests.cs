using DependencyInjectionExamples.Bad;

namespace DependencyInjectionExamples.Tests;

// Console.Out ist globaler Prozesszustand. Dieser Test darf nicht parallel
// mit anderen Tests laufen, die ebenfalls auf die Konsole zugreifen.
[CollectionDefinition("Console output", DisableParallelization = true)]
public sealed class ConsoleOutputCollection;

[Collection("Console output")]
public sealed class BadCheckoutServiceTests
{
    [Fact]
    public void CompleteCheckout_MitGueltigenDaten_SchreibtBelegAufDieKonsole()
    {
        // Arrange: Der Sender wird in CheckoutService selbst erzeugt.
        // Wir können ihn nicht ersetzen und müssen deshalb Console.Out abfangen.
        var service = new CheckoutService();
        var originalOutput = Console.Out;
        using var capturedOutput = new StringWriter();

        try
        {
            Console.SetOut(capturedOutput);

            // Act
            service.CompleteCheckout("kunde@example.com", 25m);

            // Assert: Dieser Test hängt auch vom konkreten Sender und seinem
            // Ausgabeformat ab – er ist kein isolierter Unit-Test des Service.
            Assert.Equal(
                $"Sending a receipt for {25m:F2} to kunde@example.com.{Environment.NewLine}",
                capturedOutput.ToString());
        }
        finally
        {
            // Globalen Zustand auch bei fehlgeschlagener Assertion zurücksetzen.
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void CompleteCheckout_MitNegativemBetrag_WirftException()
    {
        // Arrange: Die Validierung lässt sich ohne Sender-Zugriff prüfen.
        var service = new CheckoutService();

        // Act
        var exception = Record.Exception(() => service.CompleteCheckout("kunde@example.com", -1m));

        // Assert: Ob ein Beleg gesendet wurde, können wir hier nicht beobachten.
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }
}
