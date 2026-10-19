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
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var m = await context.Purchases
            .AsNoTracking()
            .Where(m => m.Id == id && m.UserId == userId)
            .Include(m => m.User)
            .Include(m => m.PurchaseItems)
                .ThenInclude(pi => pi.Item)
                    .ThenInclude(i => i.Sport)
            .FirstOrDefaultAsync();

        return m is null ? NotFound() : Ok(ToDTO(m));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetPurchaseDTO>> CreatePurchase(
        [FromBody] PurchaseForCreateDTO dto)
    {
        if (dto.PurchaseItems.Count == 0 ||
            dto.PurchaseItems.Select(pi => pi.ItemId).Distinct().Count() != dto.PurchaseItems.Count)
        {
            return BadRequest("Select at least one item, without duplicates.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return Unauthorized();

        var itemIds = dto.PurchaseItems.Select(pi => pi.ItemId).ToList();
        var items = await context.Items
            .Include(m => m.Sport)
            .Where(m => itemIds.Contains(m.Id))
            .ToListAsync();

        if (items.Count != itemIds.Count) return BadRequest("A selected item does not exist.");

        foreach (var pi in dto.PurchaseItems)
        {
            var m = items.First(i => i.Id == pi.ItemId);
            if (m.QuantityAvailableForPurchase < pi.Quantity)
                return Conflict($"There is not enough stock for {m.Name}.");
        }

        var purchase = new Purchase(
            user, dto.Street, dto.City, dto.Province, dto.CreditCardNumber,
            dto.CCV, dto.ExpirationDate, dto.PurchaseDescription, dto.Wrapped);

        foreach (var pi in dto.PurchaseItems)
        {
            var m = items.First(i => i.Id == pi.ItemId);
            purchase.PurchaseItems.Add(new PurchaseItem(purchase, m, pi.Quantity));
            m.QuantityAvailableForPurchase -= pi.Quantity;
        }

        purchase.TotalPrice = purchase.PurchaseItems.Sum(pi => pi.Price * pi.Quantity);
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

    private static GetPurchaseDTO ToDTO(Purchase m)
    {
        return new GetPurchaseDTO(
            m.Id, m.PurchaseDate, m.PurchaseDescription, (double)m.TotalPrice,
            m.Street, m.City, m.Province, m.CreditCardNumber, m.CCV,
            m.ExpirationDate, m.IsWrapped, m.PurchaseStatus, m.UserId,
            new PurchaseUserDTO(m.User.Name, m.User.Surname, m.User.BirthDate, m.User.Age, m.User.Gender),
            m.PurchaseItems.Select(pi => new PurchaseItemDTO(
                pi.PurchaseId, pi.ItemId, pi.Quantity, pi.Price,
                new ItemsForPurchaseDTO(
                    pi.Item.Id, pi.Item.Name, pi.Item.Brand, pi.Price,
                    pi.Item.RecommendedAge, pi.Item.Gender, pi.Item.Size,
                    pi.Item.QuantityAvailableForPurchase,
                    new SportForPurchaseDTO(
                        pi.Item.Sport.Id, pi.Item.Sport.Name,
                        pi.Item.Sport.MinimumNumberOfPlayers, pi.Item.Sport.NumberOfReferees,
                        pi.Item.Sport.Description, pi.Item.Sport.BasicRules))))
                .ToList());
    }
}
