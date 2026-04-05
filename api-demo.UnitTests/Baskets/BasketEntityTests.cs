using api_demo.Common.Exceptions;
using api_demo.Domain.Entities;
using FluentAssertions;

namespace api_demo.UnitTests.Baskets;

public class BasketEntityTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    
    [Fact]
    public void Constructor_ValidName_SetsProperties()
    {
        var basket = new Basket("Groceries", UserId);

        basket.Name.Should().Be("Groceries");
        basket.UserId.Should().Be(UserId);
        basket.IsDeleted.Should().BeFalse();
        basket.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsAppValidationException(string name)
    {
        var act = () => new Basket(name, UserId);
        act.Should().Throw<AppValidationException>();
    }
    
    [Fact]
    public void GetTotal_NoItems_ReturnsZero()
    {
        var basket = new Basket("Empty", UserId);
        basket.GetTotal().Should().Be(0m);
    }

    [Fact]
    public void GetTotal_WithItems_ReturnsSumOfItemTotals()
    {
        var basket = new Basket("Test", UserId);
        basket.AddItem("Milk", 1, 2, 3.00m); 
        basket.AddItem("Bread", 2, 1, 2.50m);

        basket.GetTotal().Should().Be(8.50m);
    }
    
    [Fact]
    public void Update_ValidName_ChangesName()
    {
        var basket = new Basket("Old Name", UserId);
        basket.Update("New Name");
        basket.Name.Should().Be("New Name");
    }

    [Fact]
    public void Update_NullName_DoesNotChangeName()
    {
        var basket = new Basket("Original", UserId);
        basket.Update(null);
        basket.Name.Should().Be("Original");
    }

    [Fact]
    public void Delete_SetsIsDeletedTrue()
    {
        var basket = new Basket("To Delete", UserId);
        basket.Delete();
        basket.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void AddItem_ValidItem_IncreasesItemCount()
    {
        var basket = new Basket("Test", UserId);
        basket.AddItem("Item 1", 1, 2, 3.00m);
        basket.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_DuplicateItemNo_ThrowsConflictException()
    {
        var basket = new Basket("Test", UserId);
        basket.AddItem("Item 1", 1, 2, 3.00m);

        // same ItemNo
        var act = () => basket.AddItem("Item 2", 1, 1, 2.00m);
        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void AddItem_DifferentItemNo_Succeeds()
    {
        var basket = new Basket("Test", UserId);
        basket.AddItem("Item 1", 1, 1, 3.00m);
        basket.AddItem("Item 2", 2, 1, 2.00m);
        basket.Items.Should().HaveCount(2);
    }
    
    [Fact]
    public void UpdateItem_ValidId_ChangesQuantity()
    {
        var basket = new Basket("Test", UserId);
        basket.AddItem("Item 1", 1, 1, 3.00m);
        var itemId = basket.Items[0].Id;

        basket.UpdateItem(itemId, 5);

        basket.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateItem_NonExistentId_ThrowsNotFoundException()
    {
        var basket = new Basket("Test", UserId);

        var act = () => basket.UpdateItem(Guid.NewGuid(), 5);
        act.Should().Throw<NotFoundException>();
    }
    
    [Fact]
    public void RemoveItem_ValidId_DecreasesItemCount()
    {
        var basket = new Basket("Test", UserId);
        basket.AddItem("Item 1", 1, 1, 3.00m);
        basket.AddItem("Item 2", 2, 1, 2.00m);
        var itemId = basket.Items[0].Id;

        basket.RemoveItem(itemId);

        basket.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_NonExistentId_ThrowsNotFoundException()
    {
        var basket = new Basket("Test", UserId);

        var act = () => basket.RemoveItem(Guid.NewGuid());
        act.Should().Throw<NotFoundException>();
    }
}
