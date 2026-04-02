namespace api_demo.Features.Baskets.Create;

public record CreateBasketRequest(string Name, List<CreateBasketItemRequest>? Items);
    
public record CreateBasketItemRequest(string ProductName, int ItemNo, int Quantity, decimal UnitPrice);