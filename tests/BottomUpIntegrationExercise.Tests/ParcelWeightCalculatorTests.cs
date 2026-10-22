using BottomUpIntegrationExercise;

namespace BottomUpIntegrationExercise.Tests;

public sealed class ParcelWeightCalculatorTests
{
    [Fact]
    public void Calculate_WithMultipleItems_CombinesRealItemWeights()
    {
        var calculator = new ParcelWeightCalculator(new ItemWeightCalculator());
        ParcelItem[] items = [new(0.4m, 2), new(0.2m, 1)];

        var weightKg = calculator.Calculate(items);

        Assert.Equal(1m, weightKg);
    }

    [Fact]
    public void Calculate_WithEmptyParcel_Throws()
    {
        var calculator = new ParcelWeightCalculator(new ItemWeightCalculator());

        var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate([]));

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void Calculate_WithInvalidSecondItem_PropagatesValidationError()
    {
        var calculator = new ParcelWeightCalculator(new ItemWeightCalculator());
        ParcelItem[] items = [new(0.4m, 2), new(0.2m, 0)];

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(items));

        Assert.Equal("item", exception.ParamName);
    }
}
