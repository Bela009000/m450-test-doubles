namespace DependencyInjectionExamples.Good;

public interface IReceiptSender
{
    void Send(string customerEmail, decimal total);
}

public sealed class CheckoutService
{
    private readonly IReceiptSender _receiptSender;

    // GOOD: Constructor injection happens here.
    // The service declares what it needs but does not choose the implementation.
    public CheckoutService(IReceiptSender receiptSender)
    {
        _receiptSender = receiptSender
            ?? throw new ArgumentNullException(nameof(receiptSender));
    }

    public void CompleteCheckout(string customerEmail, decimal total)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new ArgumentException("A customer email is required.", nameof(customerEmail));
        }

        if (total < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(total));
        }

        // CheckoutService uses the injected abstraction.
        // It does not know whether this is the real sender or a test double.
        _receiptSender.Send(customerEmail, total);
    }
}

public sealed class EmailReceiptSender : IReceiptSender
{
    public void Send(string customerEmail, decimal total)
    {
        // Production implementations may call an external email provider here.
        Console.WriteLine($"Sending a receipt for {total:F2} to {customerEmail}.");
    }
}

public sealed class RecordingReceiptSender : IReceiptSender
{
    public string? SentTo { get; private set; }

    public decimal? SentTotal { get; private set; }

    public void Send(string customerEmail, decimal total)
    {
        // This test double records the call without performing external work.
        SentTo = customerEmail;
        SentTotal = total;
    }
}

public static class CheckoutServiceFactory
{
    public static CheckoutService CreateForProduction()
    {
        var realSender = new EmailReceiptSender();

        // The application assembles its objects at the composition root.
        // The concrete dependency is injected into CheckoutService here.
        return new CheckoutService(realSender);
    }

    public static CheckoutService CreateForTest(RecordingReceiptSender testDouble)
    {
        // A test controls the dependency by injecting its own implementation.
        return new CheckoutService(testDouble);
    }
}
