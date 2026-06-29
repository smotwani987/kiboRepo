namespace Kibo.TestingFramework;

public sealed class LineItemBuilder
{
    private string _productCode = "SKU-001";
    private int _quantity = 1;
    private decimal _unitPrice = 9.99m;

    public static LineItemBuilder New() => new();

    public LineItemBuilder WithProductCode(string productCode)
    {
        _productCode = productCode;
        return this;
    }

    public LineItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public LineItemBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public LineItemRequest Build()
    {
        return new LineItemRequest(_productCode, _quantity, _unitPrice);
    }
}

public sealed record LineItemRequest(string ProductCode, int Quantity, decimal UnitPrice);
