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
    public async Task<ActionResult<GetPurchaseDTO>> GetPurchase([FromQuery] int id)
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

        return purchase is null ? NotFound() : Ok(ToDTO(purchase));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetPurchaseDTO>> CreatePurchase(
        [FromBody] PurchaseForCreateDTO dto)
    {
        var itemIds = dto.PurchaseItems.Select(item => item.ItemId).ToList();
        if (itemIds.Count == 0 || itemIds.Distinct().Count() != itemIds.Count)
            return BadRequest("Select at least one item, without duplicates.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await context.ApplicationUsers
            .FirstOrDefaultAsync(user => user.Id == userId);
        if (user is null)
            return Unauthorized();

        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var items = await context.Items
            .Where(item => itemIds.Contains(item.Id))
            .ToListAsync();
        if (items.Count != itemIds.Count)
            return BadRequest("A selected item does not exist.");

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

        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, ToDTO(purchase));
    }

    private static GetPurchaseDTO ToDTO(Purchase purchase)
    {
        var items = purchase.PurchaseItems.Select(purchaseItem => new PurchaseItemDTO(
            purchaseItem.Item.Id,
            purchaseItem.Item.Name,
            purchaseItem.Item.Brand,
            purchaseItem.Price,
            purchaseItem.Item.RecommendedAge,
            purchaseItem.Item.Gender,
            purchaseItem.Item.Size,
            purchaseItem.Quantity)).ToList();

        return new GetPurchaseDTO(
            purchase.Id,
            purchase.PurchaseDate,
            purchase.PurchaseDescription,
            purchase.TotalPrice,
            purchase.IsWrapped,
            new PurchaseUserDTO(purchase.User.Name, purchase.User.Surname),
            items);
    }
}
