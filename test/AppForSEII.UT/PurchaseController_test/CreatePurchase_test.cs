using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;

namespace AppForSEII.UT.CretePurchase_test.CreatePurchase_test;

public class CreatePurchase_test : IClassFixture<DatabaseFixture4GetOperations>
{
    private readonly DatabaseFixture4GetOperations _fixture;

    public CreatePurchase_test(DatabaseFixture4GetOperations fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> TestCasesFor_CreatePurchase()
    {
        return new List<object[]>
        {
            new object[]
            {
                CreateRequest(),
                "Select at least one item."
            },
            new object[]
            {
                CreateRequest(new PurchaseItemDTO(1, 1), new PurchaseItemDTO(1, 2)),
                "An item can only be selected once."
            },
            new object[]
            {
                CreateRequest(new PurchaseItemDTO(int.MaxValue, 1)),
                "One or more selected items do not exist."
            }
        };
    }

    [Theory]
    [MemberData(nameof(TestCasesFor_CreatePurchase))]
    public async Task CreatePurchase_InvalidItems_ReturnsValidationProblem(
        PurchaseForCreateDTO request,
        string expectedError)
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var controller = new PurchasesController(context);

        // Act
        var result = await controller.CreatePurchase(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Contains(expectedError, problemDetails.Errors["PurchaseItems"]);
    }

    [Fact]
    public async Task UnknownCustomerUserName_ReturnsBadRequest()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var controller = new PurchasesController(context);
        var request = CreateRequest(
            "missing-user",
            new PurchaseItemDTO(_fixture.FootballBoots.Id, 1));

        // Act
        var result = await controller.CreatePurchase(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task NotEnoughStock_ReturnsConflictWithoutChangingStock()
    {
        // Arrange
        var stockBefore = await GetStock();
        using var context = _fixture.CreateContext();
        var controller = new PurchasesController(context);
        var request = CreateRequest(
            new PurchaseItemDTO(_fixture.FootballBoots.Id, stockBefore + 1));

        // Act
        var result = await controller.CreatePurchase(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(stockBefore, await GetStock());
    }

    [Fact]
    public async Task SavesPurchaseAndReducesStock()
    {
        // Arrange
        var stockBefore = await GetStock();
        using var context = _fixture.CreateContext();
        var controller = new PurchasesController(context);
        var request = CreateRequest(new PurchaseItemDTO(_fixture.FootballBoots.Id, 2));

        // Act
        var result = await controller.CreatePurchase(request);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var purchase = Assert.IsType<PurchaseDetailDTO>(created.Value);
        Assert.Equal(nameof(PurchasesController.GetPurchase), created.ActionName);
        Assert.Equal(purchase.Id, Assert.IsType<int>(created.RouteValues!["id"]));
        Assert.NotEqual(default, purchase.PurchaseDate);
        Assert.Equal("Purchase test", purchase.PurchaseDescription);
        Assert.Equal(119.80m, purchase.TotalPrice);
        Assert.True(purchase.Wrapped);
        Assert.Equal("taylor@example.com", purchase.CustomerUserName);
        Assert.Equal(new PurchaseUserDTO("Taylor", "Smith"), purchase.User);
        Assert.Equal(
            new[]
            {
                new PurchaseItemDTO(
                    _fixture.FootballBoots.Id,
                    "Football Boots",
                    "Nike",
                    59.90m,
                    21,
                    Gender.Male,
                    "42",
                    "Football",
                    2)
            },
            purchase.PurchaseItems);
        Assert.Equal(stockBefore - 2, await GetStock());

        using var verificationContext = _fixture.CreateContext();
        var persistedPurchase = await verificationContext.Purchases
            .Include(savedPurchase => savedPurchase.PurchaseItems)
            .SingleAsync(savedPurchase => savedPurchase.Id == purchase.Id);
        Assert.Equal(_fixture.Customer.Id, persistedPurchase.UserId);
        Assert.Equal(PurchaseStatus.Created, persistedPurchase.PurchaseStatus);
        Assert.Equal(119.80m, persistedPurchase.TotalPrice);
        var persistedItem = Assert.Single(persistedPurchase.PurchaseItems);
        Assert.Equal(2, persistedItem.Quantity);
    }

    [Fact]
    public async Task SaveFailure_ReturnsConflictAndKeepsStock()
    {
        // Arrange
        var stockBefore = await GetStock();
        using (var context = _fixture.CreateContext())
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER \"FailPurchaseInsert\" BEFORE INSERT ON \"Purchases\" " +
                "BEGIN SELECT RAISE(ABORT, 'Simulated insert failure'); END;");
        }

        using var purchaseContext = _fixture.CreateContext();
        var controller = new PurchasesController(purchaseContext);
        var request = CreateRequest(new PurchaseItemDTO(_fixture.FootballBoots.Id, 1));

        try
        {
            // Act
            var result = await controller.CreatePurchase(request);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Equal(stockBefore, await GetStock());
        }
        finally
        {
            using var context = _fixture.CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "DROP TRIGGER IF EXISTS \"FailPurchaseInsert\";");
        }
    }

    private static PurchaseForCreateDTO CreateRequest(params PurchaseItemDTO[] items)
    {
        return CreateRequest("taylor@example.com", items);
    }

    private static PurchaseForCreateDTO CreateRequest(
        string customerUserName,
        params PurchaseItemDTO[] items)
    {
        return new PurchaseForCreateDTO(
            "Main Street 1",
            "Barcelona",
            "Barcelona",
            "4111111111111111",
            "123",
            "12/30",
            "Purchase test",
            true,
            items.ToList(),
            customerUserName);
    }

    private async Task<int> GetStock()
    {
        using var context = _fixture.CreateContext();
        return await context.Items
            .Where(item => item.Id == _fixture.FootballBoots.Id)
            .Select(item => item.QuantityAvailableForPurchase)
            .SingleAsync();
    }

}
