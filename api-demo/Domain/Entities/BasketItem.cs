using api_demo.Common.Exceptions;
using api_demo.Domain.Common;

namespace api_demo.Domain.Entities;

public class BasketItem : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid BasketId { get; private set; }
    
    public string ProductName { get; private set; } = string.Empty;
    public int ItemNo { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    public decimal GetTotal() => Quantity * UnitPrice;

    public Basket Basket { get; private set; } = null!;
    
    private BasketItem() { }

    public BasketItem(Guid basketId, string productName, int itemNo, int quantity, decimal unitPrice)
    {
        BasketId = basketId;
        SetProductName(productName);
        SetItemNo(itemNo);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public void Update(int? quantity)
    {
        if (quantity.HasValue)
        {
            SetQuantity(quantity.Value);
        }
    }
    
    private void SetProductName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new AppValidationException(nameof(ProductName), "Product name is required.");

        ProductName = name;
    }

    private void SetItemNo(int itemNo)
    {
        if (itemNo <= 0)
            throw new AppValidationException(nameof(ItemNo), "Must be greater than 0.");

        ItemNo = itemNo;
    }
    
    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new AppValidationException(nameof(Quantity), "Must be greater than 0.");

        Quantity = quantity;
    }

    private void SetUnitPrice(decimal price)
    {
        if (price <= 0)
            throw new AppValidationException(nameof(UnitPrice), "Must be greater than 0.");

        UnitPrice = price;
    }
}