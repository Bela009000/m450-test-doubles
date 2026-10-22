namespace BottomUpIntegrationExercise;

public sealed record ParcelItem(decimal UnitWeightKg, int Quantity);

public sealed record ShippingQuote(decimal TotalWeightKg, decimal ShippingCostChf);

public sealed class ItemWeightCalculator
{
    public decimal Calculate(ParcelItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.UnitWeightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(item));
        }

        if (item.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(item));
        }

        return item.UnitWeightKg * item.Quantity;
    }
}

public sealed class ParcelWeightCalculator
{
    private readonly ItemWeightCalculator _itemWeightCalculator;

    public ParcelWeightCalculator(ItemWeightCalculator itemWeightCalculator)
    {
        _itemWeightCalculator = itemWeightCalculator ?? throw new ArgumentNullException(nameof(itemWeightCalculator));
    }

    public decimal Calculate(IReadOnlyList<ParcelItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("Mindestens ein Artikel ist erforderlich.", nameof(items));
        }

        decimal totalWeightKg = 0;
        foreach (var item in items)
        {
            totalWeightKg += _itemWeightCalculator.Calculate(item);
        }

        return totalWeightKg;
    }
}

public interface IShippingRateCalculator
{
    decimal Calculate(decimal totalWeightKg);
}

public sealed class ShippingRateCalculator : IShippingRateCalculator
{
    public decimal Calculate(decimal totalWeightKg)
    {
        if (totalWeightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalWeightKg));
        }

        if (totalWeightKg <= 1m)
        {
            return 5m;
        }

        return totalWeightKg <= 5m ? 9m : 15m;
    }
}

public sealed class ShippingQuoteService
{
    private readonly ParcelWeightCalculator _parcelWeightCalculator;
    private readonly IShippingRateCalculator _shippingRateCalculator;

    public ShippingQuoteService(
        ParcelWeightCalculator parcelWeightCalculator,
        IShippingRateCalculator shippingRateCalculator)
    {
        _parcelWeightCalculator = parcelWeightCalculator ?? throw new ArgumentNullException(nameof(parcelWeightCalculator));
        _shippingRateCalculator = shippingRateCalculator ?? throw new ArgumentNullException(nameof(shippingRateCalculator));
    }

    public ShippingQuote CreateQuote(IReadOnlyList<ParcelItem> items, bool express = false)
    {
        var totalWeightKg = _parcelWeightCalculator.Calculate(items);
        var shippingCostChf = _shippingRateCalculator.Calculate(totalWeightKg);
        return new ShippingQuote(totalWeightKg, shippingCostChf + (express ? 7m : 0m));
    }
}
