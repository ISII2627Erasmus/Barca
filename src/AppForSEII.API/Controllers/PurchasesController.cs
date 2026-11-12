using System.Data;
using System.Security.Claims;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.PurchaseDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PurchasesController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(PurchaseDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseDetailDTO>> GetPurchase([FromQuery] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var purchase = await context.Purchases
            .AsNoTracking()
            .Where(purchase => purchase.Id == id && purchase.UserId == userId)
            .Include(purchase => purchase.User)
            .Include(purchase => purchase.PurchaseItems)
                .ThenInclude(purchaseItem => purchaseItem.Item)
            .FirstOrDefaultAsync();

        return purchase is null ? NotFound() : Ok(ToPurchaseDetailDTO(purchase));
    }

    [HttpPost("[action]")]
    [ProducesResponseType(typeof(PurchaseDetailDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PurchaseDetailDTO>> CreatePurchase([FromBody] PurchaseForCreateDTO dto)
    {
        var itemIds = dto.PurchaseItems.Select(item => item.ItemId).ToList();
        if (itemIds.Count == 0)
        {
            ModelState.AddModelError("PurchaseItems", "Select at least one item.");
        }

        if (itemIds.Distinct().Count() != itemIds.Count)
        {
            ModelState.AddModelError("PurchaseItems", "An item can only be selected once.");
        }

        if (ModelState.ErrorCount > 0)
        {
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await context.ApplicationUsers
            .FirstOrDefaultAsync(user => user.Id == userId);
        if (user is null)
            return Unauthorized();

        // SaveChanges makes the writes atomic, but this transaction also covers the
        // stock read/check so concurrent purchases cannot both spend the same stock.
        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var items = await context.Items
            .Where(item => itemIds.Contains(item.Id))
            .ToListAsync();
        if (items.Count != itemIds.Count)
        {
            ModelState.AddModelError("PurchaseItems", "One or more selected items do not exist.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        foreach (var selectedItem in dto.PurchaseItems)
        {
            var item = items.First(item => item.Id == selectedItem.ItemId);
            if (item.QuantityAvailableForPurchase < selectedItem.Quantity)
                return Conflict($"There is not enough stock for {item.Name}.");
        }

        var purchase = new Purchase(
            user,
            dto.Street,
            dto.City,
            dto.Province,
            dto.CreditCardNumber,
            dto.CCV,
            dto.ExpirationDate,
            dto.PurchaseDescription,
            dto.Wrapped);

        foreach (var selectedItem in dto.PurchaseItems)
        {
            var item = items.First(item => item.Id == selectedItem.ItemId);
            purchase.PurchaseItems.Add(new PurchaseItem(purchase, item, selectedItem.Quantity));
            item.QuantityAvailableForPurchase -= selectedItem.Quantity;
        }

        purchase.TotalPrice = purchase.PurchaseItems.Sum(item => item.Price * item.Quantity);
        context.Purchases.Add(purchase);

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return Conflict("The purchase could not be saved.");
        }

        return CreatedAtAction(
            nameof(GetPurchase),
            new { id = purchase.Id },
            ToPurchaseDetailDTO(purchase));
    }

    private static PurchaseDetailDTO ToPurchaseDetailDTO(Purchase purchase)
    {
        var items = purchase.PurchaseItems.Select(purchaseItem => new PurchaseItemDetailDTO(
            purchaseItem.Item.Id,
            purchaseItem.Item.Name,
            purchaseItem.Item.Brand,
            purchaseItem.Price,
            purchaseItem.Item.RecommendedAge,
            purchaseItem.Item.Gender,
            purchaseItem.Item.Size,
            purchaseItem.Quantity)).ToList();

        return new PurchaseDetailDTO(
            purchase.Id,
            purchase.PurchaseDate,
            purchase.PurchaseDescription,
            purchase.TotalPrice,
            purchase.IsWrapped,
            new PurchaseUserDTO(purchase.User.Name, purchase.User.Surname),
            items);
    }
}
