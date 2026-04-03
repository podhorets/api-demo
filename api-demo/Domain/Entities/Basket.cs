using api_demo.Common.Exceptions;
using api_demo.Domain.Common;

namespace api_demo.Domain.Entities;

public class Basket : AuditableEntity
{
    public Guid Id { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    
    public List<BasketItem> Items { get; private set; } = [];
    
    public decimal GetTotal() => Items.Sum(i => i.GetTotal());
    
    private Basket() { }

    public Basket(string name)
    {
        SetName(name);
    }
    
    public void Update(string? name)
    {
        if (name is not null)
            SetName(name);
    }
    
    public void AddItem(string productName, int itemNo, int quantity, decimal unitPrice)
    {
        var existing = Items.FirstOrDefault(i => i.ItemNo == itemNo);
        if (existing is not null)
        {
            throw new ConflictException("Product already exists in basket.");
        }

        Items.Add(new BasketItem(Id, productName, itemNo, quantity, unitPrice));
    }

    public void UpdateItem(Guid itemId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new NotFoundException("Item not found");

        item.Update(quantity);
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new NotFoundException("Item not found");

        Items.Remove(item);
    }
        
    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new AppValidationException(nameof(Name), "Basket name is required.");

        Name = name;
    }
}