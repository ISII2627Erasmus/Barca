using AppForSEII.API.Data;
using AppForSEII.API.DTOs.PurchaseDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PurchasesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(
        ApplicationDbContext context,
        ILogger<PurchasesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(PurchaseDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseDetailDTO>> GetPurchase([FromQuery] int id)
    {
        var purchase = await _context.Purchases
            .AsNoTracking()
            .Where(purchase => purchase.Id == id)
            .Select(purchase => new PurchaseDetailDTO(
                purchase.Id,
                purchase.PurchaseDate,
                purchase.PurchaseDescription,
                purchase.TotalPrice,
                purchase.IsWrapped,
                new PurchaseUserDTO(purchase.User.Name, purchase.User.Surname),
                purchase.PurchaseItems.Select(purchaseItem => new PurchaseItemDTO(
                    purchaseItem.Item.Id,
                    purchaseItem.Item.Name,
                    purchaseItem.Item.Brand,
                    purchaseItem.Price,
                    purchaseItem.Item.RecommendedAge,
                    purchaseItem.Item.Gender,
                    purchaseItem.Item.Size,
                    purchaseItem.Item.Sport.Name,
                    purchaseItem.Quantity)).ToList(),
                purchase.User.UserName!))
            .FirstOrDefaultAsync();

        if (purchase is null)
        {
            _logger.LogWarning("Purchase with ID {PurchaseId} was not found.", id);
            return NotFound();
        }

        return Ok(purchase);
    }

    [HttpPost]
    [Route("[action]")]
    [ProducesResponseType(typeof(PurchaseDetailDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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

        if (string.IsNullOrWhiteSpace(dto.CustomerUserName))
        {
            ModelState.AddModelError(nameof(dto.CustomerUserName), "Customer username is required.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(user => user.UserName == dto.CustomerUserName);
        if (user is null)
        {
            ModelState.AddModelError(nameof(dto.CustomerUserName), "The customer username was not found.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var items = await _context.Items
            .Include(item => item.Sport)
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
        _context.Purchases.Add(purchase);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Failed to save purchase for customer {CustomerUserName}.", user.UserName);
            return Conflict("The purchase could not be saved.");
        }

        return CreatedAtAction(
            nameof(GetPurchase),
            new { id = purchase.Id },
            new PurchaseDetailDTO(
                purchase.Id,
                purchase.PurchaseDate,
                purchase.PurchaseDescription,
                purchase.TotalPrice,
                purchase.IsWrapped,
                new PurchaseUserDTO(purchase.User.Name, purchase.User.Surname),
                purchase.PurchaseItems.Select(purchaseItem => new PurchaseItemDTO(
                    purchaseItem.Item.Id,
                    purchaseItem.Item.Name,
                    purchaseItem.Item.Brand,
                    purchaseItem.Price,
                    purchaseItem.Item.RecommendedAge,
                    purchaseItem.Item.Gender,
                    purchaseItem.Item.Size,
                    purchaseItem.Item.Sport.Name,
                    purchaseItem.Quantity)).ToList(),
                purchase.User.UserName!));
    }
}
