using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.GetPurchase_test.GetPurchase_test;

public class GetPurchase_test : IClassFixture<DatabaseFixture4GetOperations>
{
    private readonly DatabaseFixture4GetOperations _fixture;

    public GetPurchase_test(DatabaseFixture4GetOperations fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithFixture")]
    public async Task GetPurchase_NotFound_test()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mock = new Mock<ILogger<PurchasesController>>();
        ILogger<PurchasesController> logger = mock.Object;

        var controller = new PurchasesController(context, logger);

        // Act
        var result = await controller.GetPurchase(0);

        // Assert
        // we check that the response type is NotFound
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithFixture")]
    public async Task GetPurchase_Found_test()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mock = new Mock<ILogger<PurchasesController>>();
        ILogger<PurchasesController> logger = mock.Object;

        var controller = new PurchasesController(context, logger);

        var expectedPurchase = new PurchaseDetailDTO(
            _fixture.ExistingPurchase.Id,
            _fixture.ExistingPurchase.PurchaseDate,
            "Fixture purchase",
            119.80m,
            false,
            new PurchaseUserDTO("Taylor", "Smith"),
            new List<PurchaseItemDTO>
            {
                new(
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
            _fixture.Customer.UserName!);

        // Act
        var result = await controller.GetPurchase(_fixture.ExistingPurchase.Id);

        // Assert
        // we check that the response type is OK and obtain the purchase
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var purchaseDTOActual = Assert.IsType<PurchaseDetailDTO>(okResult.Value);

        // we check that the expected and actual are the same
        Assert.Equal(expectedPurchase, purchaseDTOActual);
    }
}