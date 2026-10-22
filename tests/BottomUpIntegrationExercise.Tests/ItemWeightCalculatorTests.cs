using BottomUpIntegrationExercise;

namespace BottomUpIntegrationExercise.Tests;

public sealed class ItemWeightCalculatorTests
{
    [Fact]
    public void Calculate_WithTwoItems_ReturnsCombinedWeight()
    {
        var calculator = new ItemWeightCalculator();

        var weight = calculator.Calculate(new ParcelItem(0.4m, 2));

        Assert.Equal(0.8m, weight);
    }

    [Fact]
    public void Calculate_WithZeroUnitWeight_Throws()
    {
        var calculator = new ItemWeightCalculator();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(new ParcelItem(0m, 2)));

        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public void Calculate_WithZeroQuantity_Throws()
    {
        var calculator = new ItemWeightCalculator();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(new ParcelItem(0.4m, 0)));

        Assert.Equal("item", exception.ParamName);
    }
}
