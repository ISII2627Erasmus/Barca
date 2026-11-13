using AppForSEII.API.Data;
using AppForSEII.API.DTOs.ItemDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(List<ItemsForPurchaseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ItemsForPurchaseDTO>>>
        GetItemsForPurchase([FromQuery] Gender? gender, [FromQuery] string? sportName)
    {
        var sportNameFilter = string.IsNullOrWhiteSpace(sportName) ? null: sportName;

        var items = await context.Items.AsNoTracking().Where(item => item.QuantityAvailableForPurchase > 0 && (!gender.HasValue || item.Gender == gender)
            && (sportNameFilter == null || item.Sport.Name == sportNameFilter))
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
                new SportForPurchaseDTO(item.Sport.Name)))
            .ToListAsync();

        return Ok(items);
    }
}
