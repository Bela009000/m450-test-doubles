using DependencyInjectionExamples.Good;

namespace DependencyInjectionExamples.Tests;

public sealed class GoodCheckoutServiceTests
{
    [Fact]
    public void CompleteCheckout_MitGueltigenDaten_UebergibtBelegAnInjiziertenSender()
    {
        // Arrange: Der Test bestimmt die Abhängigkeit selbst.
        var receiptSender = new RecordingReceiptSender();
        var service = new CheckoutService(receiptSender);

        // Act
        service.CompleteCheckout("kunde@example.com", 25m);

        // Assert: Wir prüfen die Zusammenarbeit ohne E-Mail oder globale Konsole.
        Assert.Equal("kunde@example.com", receiptSender.SentTo);
        Assert.Equal(25m, receiptSender.SentTotal);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CompleteCheckout_OhneEmail_WirftExceptionUndSendetNichts(string email)
    {
        // Arrange
        var receiptSender = new RecordingReceiptSender();
        var service = new CheckoutService(receiptSender);

        // Act
        var exception = Record.Exception(() => service.CompleteCheckout(email, 25m));

        // Assert
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal("customerEmail", ((ArgumentException)exception!).ParamName);
        Assert.Null(receiptSender.SentTo);
        Assert.Null(receiptSender.SentTotal);
    }

    [Fact]
    public void CompleteCheckout_MitNegativemBetrag_WirftExceptionUndSendetNichts()
    {
        // Arrange
        var receiptSender = new RecordingReceiptSender();
        var service = new CheckoutService(receiptSender);

        // Act
        var exception = Record.Exception(() => service.CompleteCheckout("kunde@example.com", -1m));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Null(receiptSender.SentTo);
        Assert.Null(receiptSender.SentTotal);
    }

    [Fact]
    public void Konstruktor_OhneSender_WirftException()
    {
        // Arrange: Eine notwendige Abhängigkeit fehlt.

        // Act
        var exception = Record.Exception(() => new CheckoutService(null!));

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }
}
