using BottomUpIntegrationExercise;

namespace BottomUpIntegrationExercise.Tests;

public sealed class ShippingQuoteServiceTests
{
    public static IEnumerable<object[]> QuoteCases =>
    [
        [new ParcelItem[] { new(0.4m, 2), new(0.2m, 1) }, 1m, 5m],
        [new ParcelItem[] { new(0.4m, 2), new(0.3m, 1) }, 1.1m, 9m],
        [new ParcelItem[] { new(2m, 2), new(1m, 1) }, 5m, 9m],
        [new ParcelItem[] { new(2m, 2), new(1m, 2) }, 6m, 15m]
    ];

    [Fact]
    public void CreateQuote_WithRatePlaceholder_PassesCalculatedWeightToRate()
    {
        var rate = new RecordingRatePlaceholder();
        var service = new ShippingQuoteService(
            new ParcelWeightCalculator(new ItemWeightCalculator()), rate);
        ParcelItem[] items = [new(0.4m, 2), new(0.2m, 1)];

        var quote = service.CreateQuote(items);

        Assert.Equal(1m, quote.TotalWeightKg);
        Assert.Equal(42m, quote.ShippingCostChf);
        Assert.Equal(1m, rate.ReceivedWeightKg);
        Assert.Equal(1, rate.CallCount);
    }

    [Theory]
    [MemberData(nameof(QuoteCases))]
    public void CreateQuote_WithRealComponents_ReturnsWeightAndTariff(
        ParcelItem[] items, decimal expectedWeightKg, decimal expectedCostChf)
    {
        var service = CreateService();

        var quote = service.CreateQuote(items);

        Assert.Equal(expectedWeightKg, quote.TotalWeightKg);
        Assert.Equal(expectedCostChf, quote.ShippingCostChf);
    }

    [Fact]
    public void CreateQuote_WithEmptyParcel_Throws()
    {
        var service = CreateService();

        var exception = Assert.Throws<ArgumentException>(() => service.CreateQuote([]));

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void CreateQuote_WithExpress_AddsSevenFrancsAtOneKgBoundary()
    {
        var service = CreateService();
        ParcelItem[] items = [new(0.4m, 2), new(0.2m, 1)];

        var standardQuote = service.CreateQuote(items);
        var expressQuote = service.CreateQuote(items, express: true);

        Assert.Equal(1m, standardQuote.TotalWeightKg);
        Assert.Equal(1m, expressQuote.TotalWeightKg);
        Assert.Equal(5m, standardQuote.ShippingCostChf);
        Assert.Equal(12m, expressQuote.ShippingCostChf);
    }

    private static ShippingQuoteService CreateService() =>
        new(new ParcelWeightCalculator(new ItemWeightCalculator()), new ShippingRateCalculator());

    private sealed class RecordingRatePlaceholder : IShippingRateCalculator
    {
        public decimal? ReceivedWeightKg { get; private set; }

        public int CallCount { get; private set; }

        public decimal Calculate(decimal totalWeightKg)
        {
            ReceivedWeightKg = totalWeightKg;
            CallCount++;
            return 42m;
        }
    }
}
