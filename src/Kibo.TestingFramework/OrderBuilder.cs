namespace Kibo.TestingFramework;

public sealed class OrderBuilder
{
    private readonly List<LineItemRequest> _lineItems = new()
    {
        LineItemBuilder.New().Build()
    };

    private string _customerEmail = "customer@example.com";

    public static OrderBuilder New() => new();

    public OrderBuilder WithCustomerEmail(string customerEmail)
    {
        _customerEmail = customerEmail;
        return this;
    }

    public OrderBuilder WithLineItem(LineItemBuilder lineItem)
    {
        _lineItems.Add(lineItem.Build());
        return this;
    }

    public OrderBuilder WithLineItem(LineItemRequest lineItem)
    {
        _lineItems.Add(lineItem);
        return this;
    }

    public OrderBuilder WithLineItems(params LineItemBuilder[] lineItems)
    {
        foreach (var lineItem in lineItems)
        {
            _lineItems.Add(lineItem.Build());
        }

        return this;
    }

    public OrderBuilder WithLineItems(params LineItemRequest[] lineItems)
    {
        _lineItems.AddRange(lineItems);
        return this;
    }

    public OrderBuilder WithoutLineItems()
    {
        _lineItems.Clear();
        return this;
    }

    public OrderRequest Build()
    {
        return new OrderRequest(_customerEmail, _lineItems.ToArray());
    }
}

public sealed record OrderRequest(string CustomerEmail, IReadOnlyCollection<LineItemRequest> LineItems);
