namespace DependencyInjectionExamples.Bad;

public sealed class CheckoutService
{
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

        // BAD: The service creates its own concrete dependency.
        // A unit test cannot replace this sender with a fake or recording sender.
        var receiptSender = new EmailReceiptSender();

        // A unit test of CheckoutService now also depends on the email implementation.
        receiptSender.Send(customerEmail, total);
    }
}

public sealed class EmailReceiptSender
{
    public void Send(string customerEmail, decimal total)
    {
        // Imagine that this connects to a real email provider.
        Console.WriteLine($"Sending a receipt for {total:F2} to {customerEmail}.");
    }
}
