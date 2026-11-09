using System.Security.Claims;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.AspNetCore.Http;

namespace AppForSEII.UT.CretePurchase_test.CreatePurchase_test;

public class CreatePurchase_test : IClassFixture<Databasefixture4GetOprations>
{
    private readonly Databasefixture4GetOprations _fixture;

    public CreatePurchase_test(Databasefixture4GetOprations fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WithoutItems_ReturnsBadRequest()
    {
        var result = await CreatePurchase(_fixture.Customer.Id);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task WithDuplicateItems_ReturnsBadRequest()
    {
        var itemId = _fixture.FootballBoots.Id;
        var result = await CreatePurchase(_fixture.Customer.Id, new PurchaseItemDTO(itemId, 1), new PurchaseItemDTO(itemId, 2));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task WithoutUser_ReturnsUnauthorized()
    {
        var result = await CreatePurchase(null, new PurchaseItemDTO(_fixture.FootballBoots.Id, 1));

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task UserDoesNotExist_ReturnsUnauthorized()
    {
        var result = await CreatePurchase("missing-user", new PurchaseItemDTO(_fixture.FootballBoots.Id, 1));

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task ItemDoesNotExist_ReturnsBadRequest()
    {
        var result = await CreatePurchase(_fixture.Customer.Id, new PurchaseItemDTO(int.MaxValue, 1));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task NotEnoughStock_ReturnsConflictWithoutChangingStock()
    {
        var stockBefore = await GetStock();
        var result = await CreatePurchase(_fixture.Customer.Id, new PurchaseItemDTO(_fixture.FootballBoots.Id, stockBefore + 1));

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(stockBefore, await GetStock());
    }

    [Fact]
    public async Task SavesPurchaseAndReducesStock()
    {
        var stockBefore = await GetStock();
        var result = await CreatePurchase(_fixture.Customer.Id, new PurchaseItemDTO(_fixture.FootballBoots.Id, 2));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var purchase = Assert.IsType<GetPurchaseDTO>(created.Value);

        var expected = new GetPurchaseDTO(purchase.Id, purchase.PurchaseDate, "Purchase test", 119.80m, true,
            new PurchaseUserDTO("Taylor", "Smith"),
            new List<PurchaseItemDetailDTO>
            {
                new(_fixture.FootballBoots.Id,"Football Boots", "Nike", 59.90m, 21, Gender.Male, "42", 2)
            });

        Assert.Equal(expected, purchase);
        Assert.Equal(nameof(PurchasesController.GetPurchase), created.ActionName);
        Assert.Equal(purchase.Id, Assert.IsType<int>(created.RouteValues!["id"]));
        Assert.Equal(stockBefore - 2, await GetStock());
    }

    [Fact]
    public async Task SaveFailure_ReturnsConflictAndKeepsStock()
    {
        var stockBefore = await GetStock();
        using (var context = _fixture.CreateContext())
        {
            await context.Database.ExecuteSqlRawAsync("CREATE TRIGGER \"FailPurchaseInsert\" BEFORE INSERT ON \"Purchases\" " +
                                                      "BEGIN SELECT RAISE(ABORT, 'Simulated insert failure'); END;");
        }

        try
        {
            var result = await CreatePurchase(_fixture.Customer.Id, new PurchaseItemDTO(_fixture.FootballBoots.Id, 1));

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Equal(stockBefore, await GetStock());
        }
        finally
        {
            using var context = _fixture.CreateContext();
            await context.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS \"FailPurchaseInsert\";");
        }
    }

    private async Task<ActionResult<GetPurchaseDTO>> CreatePurchase(
        string? userId,
        params PurchaseItemDTO[] items)
    {
        using var context = _fixture.CreateContext();
        var controller = new PurchasesController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreateUser(userId) }
            }
        };

        return await controller.CreatePurchase(new PurchaseForCreateDTO(
            "Main Street 1",
            "Barcelona",
            "Barcelona",
            "4111111111111111",
            "123",
            "12/30",
            "Purchase test",
            true,
            items.ToList()));
    }

    private async Task<int> GetStock()
    {
        using var context = _fixture.CreateContext();
        return await context.Items
            .Where(item => item.Id == _fixture.FootballBoots.Id)
            .Select(item => item.QuantityAvailableForPurchase)
            .SingleAsync();
    }

    private static ClaimsPrincipal CreateUser(string? userId) => userId is null
        ? new ClaimsPrincipal()
        : new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            "UnitTest"));
}
