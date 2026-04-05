using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using api_demo.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace api_demo.IntegrationTests.Tests;

public class BasketsEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly AuthHelper _auth = new(factory.CreateClient());

    private async Task AuthorizedTokenAsync()
    {
        var token = await _auth.SignupAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    [Fact]
    public async Task CreateBasket_WithItems_Returns201()
    {
        await AuthorizedTokenAsync();

        var resp = await _client.PostAsJsonAsync("/api/v1/baskets", new
        {
            Name = "Basket 1",
            Items = new[]
            {
                new { ProductName = "Item 1",  ItemNo = 1, Quantity = 2, UnitPrice = 3.50m },
                new { ProductName = "Item 2", ItemNo = 2, Quantity = 1, UnitPrice = 2.99m }
            }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location.Should().NotBeNull();
        var basket = await resp.Content.ReadFromJsonAsync<JsonElement>();
        basket.GetProperty("name").GetString().Should().Be("Basket 1");
        basket.GetProperty("items").GetArrayLength().Should().Be(2);
        basket.GetProperty("total").GetDecimal().Should().Be(9.99m);
    }

    [Fact]
    public async Task CreateBasket_NoAuth_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var resp = await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Test" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBasket_EmptyName_Returns400()
    {
        await AuthorizedTokenAsync();

        var resp = await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await resp.Content.ReadFromJsonAsync<JsonElement>();
        error.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");
        error.GetProperty("errors").EnumerateObject().Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task GetBasket_Returns200WithItemsAndTotal()
    {
        await AuthorizedTokenAsync();

        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new
        {
            Name = "Basket 1",
            Items = new[] { new { ProductName = "Item 1", ItemNo = 1, Quantity = 3, UnitPrice = 5.00m } }
        });
        var basketId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        var resp = await _client.GetAsync($"/api/v1/baskets/{basketId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var basket = await resp.Content.ReadFromJsonAsync<JsonElement>();
        basket.GetProperty("name").GetString().Should().Be("Basket 1");
        basket.GetProperty("items").GetArrayLength().Should().Be(1);
        basket.GetProperty("total").GetDecimal().Should().Be(15.00m);
    }

    [Fact]
    public async Task GetBasket_NonExistentId_Returns404()
    {
        await AuthorizedTokenAsync();

        var resp = await _client.GetAsync($"/api/v1/baskets/{Guid.NewGuid()}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task SearchBaskets_ReturnsPaginatedResults()
    {
        await AuthorizedTokenAsync();
        await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Basket 1" });
        await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Basket 2" });

        var resp = await _client.GetAsync("/api/v1/baskets?page=1&pageSize=10");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        result.GetProperty("page").GetInt32().Should().Be(1);
        result.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task SearchBaskets_ByName_ReturnsMatchingOnly()
    {
        await AuthorizedTokenAsync();
        await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Jeans black" });
        await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Shoes" });

        var resp = await _client.GetAsync("/api/v1/baskets?query=Jeans");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").EnumerateArray().Should().AllSatisfy(b =>
            b.GetProperty("name").GetString()!.Should().Contain("Jeans"));
    }

    [Fact]
    public async Task SearchBaskets_ExcludesOtherUsersBaskets()
    {
        var tokenA = await _auth.SignupAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "User A basket" });

        var tokenB = await _auth.SignupAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var resp = await _client.GetAsync("/api/v1/baskets");

        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("totalCount").GetInt32().Should().Be(0);
    }
    
    [Fact]
    public async Task UpdateBasket_ChangesName_Returns200()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Old Name" });
        var basketId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        var resp = await _client.PutAsJsonAsync($"/api/v1/baskets/{basketId}", new { Name = "New Name" });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var basket = await resp.Content.ReadFromJsonAsync<JsonElement>();
        basket.GetProperty("name").GetString().Should().Be("New Name");
    }
    
    [Fact]
    public async Task DeleteBasket_SoftDeletes_Returns204AndThenNotFound()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "To Delete" });
        var basketId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        var delResp = await _client.DeleteAsync($"/api/v1/baskets/{basketId}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await _client.GetAsync($"/api/v1/baskets/{basketId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_Returns200WithUpdatedBasket()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Basket" });
        var basketId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        var resp = await _client.PostAsJsonAsync($"/api/v1/baskets/{basketId}/items",
            new { ProductName = "Item 123", ItemNo = 1, Quantity = 4, UnitPrice = 1.25m });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var basket = await resp.Content.ReadFromJsonAsync<JsonElement>();
        basket.GetProperty("items").GetArrayLength().Should().Be(1);
        basket.GetProperty("total").GetDecimal().Should().Be(5.00m);
    }

    [Fact]
    public async Task AddItem_DuplicateItemNo_Returns409()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new
        {
            Name = "Basket",
            Items = new[] { new { ProductName = "Item 1", ItemNo = 1, Quantity = 2, UnitPrice = 3.50m } }
        });
        var basketId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        // Same ItemNo
        var resp = await _client.PostAsJsonAsync($"/api/v1/baskets/{basketId}/items",
            new { ProductName = "Item 2", ItemNo = 1, Quantity = 1, UnitPrice = 2.99m });

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateItem_ChangesQuantity_Returns200()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new
        {
            Name = "Basket",
            Items = new[] { new { ProductName = "Item 1", ItemNo = 1, Quantity = 1, UnitPrice = 3.00m } }
        });
        var basketJson = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var basketId = basketJson.GetProperty("id").GetString();
        var itemId = basketJson.GetProperty("items")[0].GetProperty("id").GetString();

        var resp = await _client.PutAsJsonAsync(
            $"/api/v1/baskets/{basketId}/items/{itemId}", new { Quantity = 5 });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var basket = await resp.Content.ReadFromJsonAsync<JsonElement>();
        basket.GetProperty("items")[0].GetProperty("quantity").GetInt32().Should().Be(5);
        basket.GetProperty("total").GetDecimal().Should().Be(15.00m);
    }

    [Fact]
    public async Task RemoveItem_Returns200WithUpdatedBasket()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new
        {
            Name = "Basket",
            Items = new[]
            {
                new { ProductName = "Item 1",  ItemNo = 1, Quantity = 1, UnitPrice = 3.00m },
                new { ProductName = "Item 2", ItemNo = 2, Quantity = 1, UnitPrice = 2.00m }
            }
        });
        var basketJson = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var basketId = basketJson.GetProperty("id").GetString();
        var itemId = basketJson.GetProperty("items")[0].GetProperty("id").GetString();

        var resp = await _client.DeleteAsync($"/api/v1/baskets/{basketId}/items/{itemId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var basket = await resp.Content.ReadFromJsonAsync<JsonElement>();
        basket.GetProperty("items").GetArrayLength().Should().Be(1);
        basket.GetProperty("total").GetDecimal().Should().Be(2.00m);
    }

    [Fact]
    public async Task RemoveItem_NonExistentItem_Returns404()
    {
        await AuthorizedTokenAsync();
        var createResp = await _client.PostAsJsonAsync("/api/v1/baskets", new { Name = "Basket" });
        var basketId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        var resp = await _client.DeleteAsync(
            $"/api/v1/baskets/{basketId}/items/{Guid.NewGuid()}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
