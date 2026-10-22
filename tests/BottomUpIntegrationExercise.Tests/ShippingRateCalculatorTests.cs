using BottomUpIntegrationExercise;

namespace BottomUpIntegrationExercise.Tests;

public sealed class ShippingRateCalculatorTests
{
    public static IEnumerable<object[]> RateCases =>
    [
        [1m, 5m],
        [1.01m, 9m],
        [5m, 9m],
        [5.01m, 15m]
    ];

    [Theory]
    [MemberData(nameof(RateCases))]
    public void Calculate_AtTariffBoundaries_ReturnsExpectedCost(
        decimal weightKg, decimal expectedCostChf)
    {
        var calculator = new ShippingRateCalculator();

        var costChf = calculator.Calculate(weightKg);

        Assert.Equal(expectedCostChf, costChf);
    }

    [Fact]
    public void Calculate_WithZeroWeight_Throws()
    {
        var calculator = new ShippingRateCalculator();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(0m));

        Assert.Equal("totalWeightKg", exception.ParamName);
    }
}
