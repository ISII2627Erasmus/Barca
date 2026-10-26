using AppForSEII.API.Data;
using AppForSEII.API.DTOs.PurchaseDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("[action]")]
    [ProducesResponseType(
        typeof(List<ItemsForPurchaseDTO>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemsForPurchaseDTO>>>
        GetItemsForPurchase(
            [FromQuery] Gender? gender,
            [FromQuery] string? sportName,
            CancellationToken cancellationToken)
    {
        var query = context.Items
            .AsNoTracking()
            .Where(item => item.QuantityAvailableForPurchase > 0);

        if (gender.HasValue)
        {
            query = query.Where(item => item.Gender == gender.Value);
        }

        if (!string.IsNullOrWhiteSpace(sportName))
        {
            query = query.Where(item => item.Sport.Name == sportName);
        }

        var items = await query
            .OrderBy(item => item.Gender)
            .ThenBy(item => item.Name)
            .Select(item => new ItemsForPurchaseDTO(
                item.Id,
                item.Name,
                item.Brand,
                item.Price,
                item.RecommendedAge,
                item.Gender,
                item.Size,
                new SportForPurchaseDTO(item.Sport.Id, item.Sport.Name)))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}
