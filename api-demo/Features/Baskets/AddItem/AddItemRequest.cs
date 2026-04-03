namespace api_demo.Features.Baskets.AddItem;

public record AddItemRequest(
    string ProductName,
    int ItemNo,
    int Quantity,
    decimal UnitPrice);
