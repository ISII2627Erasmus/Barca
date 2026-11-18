using System.Security.Claims;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.AspNetCore.Http;

namespace AppForSEII.UT.GetPurchase_test.GetPurchase_test;

public class GetPurchase_test : IClassFixture<DatabaseFixture4GetOperations>
{
    private readonly DatabaseFixture4GetOperations _fixture;

    public GetPurchase_test(DatabaseFixture4GetOperations fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task WithoutUser_ReturnsUnauthorized()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var controller = CreateController(context, userId: null);

        // Act
        var result = await controller.GetPurchase(_fixture.ExistingPurchase.Id);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task OwnedByAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var controller = CreateController(context, "another-user");

        // Act
        var result = await controller.GetPurchase(_fixture.ExistingPurchase.Id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ReturnsPurchaseForCurrentUser()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var controller = CreateController(context, _fixture.Customer.Id);
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
            });

        // Act
        var result = await controller.GetPurchase(_fixture.ExistingPurchase.Id);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<PurchaseDetailDTO>(response.Value);
        Assert.Equal(expected, actual);
    }

    private static PurchasesController CreateController(
        ApplicationDbContext context,
        string? userId)
    {
        return new PurchasesController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(userId)
                }
            }
        };
    }

    private static ClaimsPrincipal CreateUser(string? userId)
    {
        if (userId is null)
        {
            return new ClaimsPrincipal();
        }

        return new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            "UnitTest"));
    }
}
