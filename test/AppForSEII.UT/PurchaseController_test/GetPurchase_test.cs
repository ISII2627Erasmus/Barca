using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppForSEII.UT.GetPurchase_test.GetPurchase_test;

public class GetPurchase_test : IClassFixture<DatabaseFixture4GetOperations>
{
    private readonly DatabaseFixture4GetOperations _fixture;

    public GetPurchase_test(DatabaseFixture4GetOperations fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnsPurchaseById()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var controller = new PurchasesController(context, NullLogger<PurchasesController>.Instance);
        var expected = new PurchaseDetailDTO(
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
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<PurchaseDetailDTO>(response.Value);
        Assert.Equal(expected, actual);
    }

}
