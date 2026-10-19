using AppForSEII.API.Data;
using AppForSEII.API.DTOs.PurchaseDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ItemsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("[action]")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ItemsForPurchaseDTO>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ItemsForPurchaseDTO>>>
        GetItemsForPurchase(
            [FromQuery] Gender? gender,
            CancellationToken cancellationToken)
    {
        var query = context.Items.AsNoTracking();

        if (gender.HasValue)
        {
            query = query.Where(item => item.Gender == gender.Value);
        }

        var items = await query
            .Include(item => item.Sport)
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
                item.QuantityAvailableForPurchase,
                new SportForPurchaseDTO(item.Sport.Id, item.Sport.Name, item.Sport.MinimumNumberOfPlayers, item.Sport.NumberOfReferees,
                item.Sport.Description, item.Sport.BasicRules)))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}
