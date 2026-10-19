using AppForSEII.API.Data;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PurchasesController(ApplicationDbContext context)
    : ControllerBase
{
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(GetPurchaseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetPurchaseDTO>> GetPurchase(
        [FromQuery] int id,
        CancellationToken cancellationToken)
    {
        var purchase = await context.Purchases
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Include(m => m.User)
            .Include(m => m.PurchaseItems)
                .ThenInclude(pi => pi.Item)
                    .ThenInclude(i => i.Sport)
            .Select(m => new GetPurchaseDTO(
                m.Id,
                m.PurchaseDate,
                m.PurchaseDescription,
                (double)m.TotalPrice,
                m.Street,
                m.City,
                m.Province,
                m.CreditCardNumber,
                m.CCV,
                m.ExpirationDate,
                m.IsWrapped,
                m.PurchaseStatus,
                m.UserId,
                new PurchaseUserDTO(
                    m.User.Name,
                    m.User.Surname,
                    m.User.BirthDate,
                    m.User.Age,
                    m.User.Gender),
                m.PurchaseItems
                    .Select(pi => new PurchaseItemDTO(
                        pi.PurchaseId,
                        pi.ItemId,
                        pi.Quantity,
                        pi.Price,
                        new ItemForPurchaseDTO(
                            pi.Item.Id,
                            pi.Item.Name,
                            pi.Item.Price,
                            pi.Item.RecommendedAge,
                            pi.Item.Gender,
                            pi.Item.Size,
                            pi.Item.QuantityAvailableForPurchase,
                            new SportForPurchaseDTO(
                                pi.Item.Sport.Id,
                                pi.Item.Sport.Name,
                                pi.Item.Sport.MinimumNumberOfPlayers,
                                pi.Item.Sport.NumberOfReferees,
                                pi.Item.Sport.Description,
                                pi.Item.Sport.BasicRules))))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (purchase is null)
        {
            return NotFound();
        }

        return Ok(purchase);
    }
}