using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ItemDTOs;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.CretePurchase_test.CreatePurchase_test;

public class CreatePurchase_test : IClassFixture<DatabaseFixture4GetOperations>
{
    private const string _userName = "taylor@example.com";

    private readonly DatabaseFixture4GetOperations _fixture;

    public CreatePurchase_test(DatabaseFixture4GetOperations fixture)
    {
        _fixture = fixture;
    }

    public static TheoryData<PurchaseForCreateDTO, string> TestCasesFor_CreatePurchase => new()
    {
        {   new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30", "Purchase test", true,
                new List<PurchaseItemDTO>(), _userName), "Select at least one item."},

        {   new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30", "Purchase test", true,
                new List<PurchaseItemDTO> { new(1, 1), new(1, 2) }, _userName), "An item can only be selected once."},

        {   new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30","Purchase test", true,
                new List<PurchaseItemDTO> { new(int.MaxValue, 1) }, _userName), "One or more selected items do not exist."},
        
        {   new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30", "Purchase test", true,
                new List<PurchaseItemDTO> { new(1, 1) }, "missing-user"), "The customer username was not found."},

        {   new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30", "Purchase test", true,
                new List<PurchaseItemDTO> { new(1, 1) }, "   "),"Customer username is required."}
    };

    [Theory]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithFixture")]
    [MemberData(nameof(TestCasesFor_CreatePurchase))]
    public async Task CreatePurchase_Error_test(PurchaseForCreateDTO purchaseDTO, string errorExpected)
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mock = new Mock<ILogger<PurchasesController>>();
        var controller = new PurchasesController(context, mock.Object);

        // Act
        var result = await controller.CreatePurchase(purchaseDTO);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        var errorActual = problemDetails.Errors.First().Value[0];
        Assert.StartsWith(errorExpected, errorActual);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithFixture")]
    public async Task CreatePurchase_NotEnoughStock_test()
    {
        // Read stock through the Items controller, the same API operation used to browse items.
        using var stockContextBefore = _fixture.CreateContext();
        var stockResultBefore = await new ItemsController(stockContextBefore).GetItemsForPurchase(null, null);
        var stockItemsBefore = Assert.IsType<List<ItemsForPurchaseDTO>>(Assert.IsType<OkObjectResult>(stockResultBefore.Result).Value);
        var stockBefore = Assert.Single(stockItemsBefore, item => item.Id == _fixture.FootballBoots.Id).QuantityAvailableForPurchase;

        // Arrange
        using var context = _fixture.CreateContext();
        var mock = new Mock<ILogger<PurchasesController>>();
        var controller = new PurchasesController(context, mock.Object);
        var purchaseDTO = new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30", "Purchase test", true,
            new List<PurchaseItemDTO> { new(_fixture.FootballBoots.Id, stockBefore + 1) }, _userName);

        // Act
        var result = await controller.CreatePurchase(purchaseDTO);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var message = Assert.IsType<string>(conflictResult.Value);
        Assert.StartsWith("There is not enough stock for Football Boots", message);

        using var stockContextAfter = _fixture.CreateContext();
        var stockResultAfter = await new ItemsController(stockContextAfter).GetItemsForPurchase(null, null);
        var stockItemsAfter = Assert.IsType<List<ItemsForPurchaseDTO>>(Assert.IsType<OkObjectResult>(stockResultAfter.Result).Value);
        var stockAfter = Assert.Single(stockItemsAfter, item => item.Id == _fixture.FootballBoots.Id).QuantityAvailableForPurchase;
        Assert.Equal(stockBefore, stockAfter);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithFixture")]
    public async Task CreatePurchase_Success_test()
    {
        using var stockContextBefore = _fixture.CreateContext();
        var stockResultBefore = await new ItemsController(stockContextBefore).GetItemsForPurchase(null, null);
        var stockItemsBefore = Assert.IsType<List<ItemsForPurchaseDTO>>(Assert.IsType<OkObjectResult>(stockResultBefore.Result).Value);
        var stockBefore = Assert.Single(stockItemsBefore, item => item.Id == _fixture.FootballBoots.Id).QuantityAvailableForPurchase;

        // Arrange
        using var context = _fixture.CreateContext();
        var mock = new Mock<ILogger<PurchasesController>>();
        var controller = new PurchasesController(context, mock.Object);
        var purchaseDTO = new PurchaseForCreateDTO("Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30","Purchase test", true,
            new List<PurchaseItemDTO> { new(_fixture.FootballBoots.Id, 2) }, _userName);

        // Act
        var result = await controller.CreatePurchase(purchaseDTO);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var actualPurchaseDetailDTO = Assert.IsType<PurchaseDetailDTO>(createdResult.Value);
        var expectedPurchaseDetailDTO = new PurchaseDetailDTO(
            actualPurchaseDetailDTO.Id, actualPurchaseDetailDTO.PurchaseDate,"Purchase test", 119.80m, true, "Taylor", "Smith",
            new List<PurchaseItemDTO>{new(_fixture.FootballBoots.Id,"Football Boots", "Nike", 59.90m, 21, Gender.Male, "42", "Football", 2)},_userName);

        Assert.Equal(expectedPurchaseDetailDTO, actualPurchaseDetailDTO);
        Assert.Equal(nameof(PurchasesController.GetPurchase), createdResult.ActionName);
        Assert.Equal(actualPurchaseDetailDTO.Id, Assert.IsType<int>(createdResult.RouteValues!["id"]));

    }


    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithFixture")]
    public async Task CreatePurchase_DbContextException_test()
    {
        // Arrange
        using var contextBefore = _fixture.CreateContext();
        var purchaseCountBefore = await contextBefore.Purchases.CountAsync();
        await contextBefore.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER \"FailPurchaseInsert\" BEFORE INSERT ON \"Purchases\" " +
            "BEGIN SELECT RAISE(ABORT, 'Simulated insert failure'); END;");

        try
        {
            using var context = _fixture.CreateContext();
            var mock = new Mock<ILogger<PurchasesController>>();
            var controller = new PurchasesController(context, mock.Object);
            var purchaseDTO = new PurchaseForCreateDTO(
                "Main Street 1", "Barcelona", "Barcelona", "4111111111111111", "123", "12/30",
                "Purchase test", true,
                new List<PurchaseItemDTO> { new(_fixture.FootballBoots.Id, 1) }, _userName);

            // Act
            var result = await controller.CreatePurchase(purchaseDTO);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Equal("The purchase could not be saved.", conflictResult.Value);

            using var verificationContext = _fixture.CreateContext();
            Assert.Equal(purchaseCountBefore, await verificationContext.Purchases.CountAsync());
        }
        finally
        {
            using var cleanupContext = _fixture.CreateContext();
            await cleanupContext.Database.ExecuteSqlRawAsync(
                "DROP TRIGGER IF EXISTS \"FailPurchaseInsert\";");
        }
    }
}
