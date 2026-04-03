using api_demo.Common.Models;
using api_demo.Features.Baskets.AddItem;
using api_demo.Features.Baskets.Create;
using api_demo.Features.Baskets.Delete;
using api_demo.Features.Baskets.GetById;
using api_demo.Features.Baskets.RemoveItem;
using api_demo.Features.Baskets.Search;
using api_demo.Features.Baskets.Shared;
using api_demo.Features.Baskets.Update;
using api_demo.Features.Baskets.UpdateItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_demo.Features.Baskets;

[ApiController]
[Route("api/v1/baskets")]
[Authorize]
[Produces("application/json")]
public class BasketsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateBasketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBasketRequest request,
        [FromServices] CreateBasketHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetBasketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetBasketHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<SearchBasketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchBasketsRequest request,
        [FromServices] SearchBasketsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateBasketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateBasketRequest request,
        [FromServices] UpdateBasketHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteBasketHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(id, ct);
        return NoContent();
    }
    
    [HttpPost("{basketId:guid}/items")]
    [ProducesResponseType(typeof(BasketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddItem(
        Guid basketId,
        [FromBody] AddItemRequest request,
        [FromServices] AddItemHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(basketId, request, ct);
        return Ok(result);
    }

    [HttpPut("{basketId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(BasketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(
        Guid basketId,
        Guid itemId,
        [FromBody] UpdateItemRequest request,
        [FromServices] UpdateItemHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(basketId, itemId, request, ct);
        return Ok(result);
    }

    [HttpDelete("{basketId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(BasketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(
        Guid basketId,
        Guid itemId,
        [FromServices] RemoveItemHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(basketId, itemId, ct);
        return Ok(result);
    }
}