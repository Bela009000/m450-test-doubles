using Moq;

namespace TestDoublesExercise.MSTest.Tests;

// Kleines, unabhängiges Beispiel. Für die Reservierungsaufgabe schreibt ihr eigene Doubles.
public interface IStockReader
{
    int GetStock(string productCode);
}

public interface IOrderNotifier
{
    void Notify(string productCode);
}

public sealed class StockCheckService(IStockReader stockReader, IOrderNotifier notifier)
{
    public bool TryOrder(string productCode)
    {
        if (stockReader.GetStock(productCode) <= 0)
        {
            return false;
        }

        notifier.Notify(productCode);
        return true;
    }
}

[TestClass]
public sealed class TestDoubleExamples
{
    [TestMethod]
    public void HandwrittenStub_ControlsTheStockResponse()
    {
        // Arrange: Der Stub liefert den Wert, den dieser Test braucht.
        var stock = new FixedStockStub(0);
        var notifier = new RecordingOrderSpy();
        var service = new StockCheckService(stock, notifier);

        // Act
        var result = service.TryOrder("BOOK-1");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, notifier.NotifiedProductCodes.Count);
    }

    [TestMethod]
    public void HandwrittenSpy_RecordsTheNotification()
    {
        // Arrange: Der Spy sammelt Aufrufe, statt eine echte Nachricht zu senden.
        var notifier = new RecordingOrderSpy();
        var service = new StockCheckService(new FixedStockStub(2), notifier);

        // Act
        var result = service.TryOrder("BOOK-1");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, notifier.NotifiedProductCodes.Count);
        Assert.AreEqual("BOOK-1", notifier.NotifiedProductCodes[0]);
    }

    [TestMethod]
    public void Moq_ConfiguresAndVerifiesTheStockReader()
    {
        // Arrange: Moq liefert einen Wert und zeichnet den Methodenaufruf auf.
        var stock = new Mock<IStockReader>();
        stock.Setup(reader => reader.GetStock("BOOK-1")).Returns(2);
        var notifier = new RecordingOrderSpy();
        var service = new StockCheckService(stock.Object, notifier);

        // Act
        var result = service.TryOrder("BOOK-1");

        // Assert
        Assert.IsTrue(result);
        stock.Verify(reader => reader.GetStock("BOOK-1"), Times.Once());
        Assert.AreEqual(1, notifier.NotifiedProductCodes.Count);
        Assert.AreEqual("BOOK-1", notifier.NotifiedProductCodes[0]);
    }

    private sealed class FixedStockStub(int availableItems) : IStockReader
    {
        public int GetStock(string productCode) => availableItems;
    }

    private sealed class RecordingOrderSpy : IOrderNotifier
    {
        private readonly List<string> _notifiedProductCodes = [];

        public IReadOnlyList<string> NotifiedProductCodes => _notifiedProductCodes;

        public void Notify(string productCode) => _notifiedProductCodes.Add(productCode);
    }
}
