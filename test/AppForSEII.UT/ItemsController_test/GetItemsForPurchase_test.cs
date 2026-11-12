using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ItemDTOs;

namespace AppForSEII.UT.ItemsController_test;

public class GetItemsForPurchase_test : IClassFixture<DatabaseFixture4GetOperations>
{
    private readonly DatabaseFixture4GetOperations _fixture;

    public GetItemsForPurchase_test(DatabaseFixture4GetOperations fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FiltersByGenderAndSport()
    {
        // Arrange
        var expected = new List<ItemsForPurchaseDTO>
        {
            new(
                _fixture.FootballBoots.Id,
                "Football Boots",
                "Nike",
                59.90m,
                21,
                Gender.Male,
                "42",
                new SportForPurchaseDTO("Football"))
        };

        // Act
        var result = await GetItems(Gender.Male, "Football");

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FiltersByGenderOnly()
    {
        // Arrange
        var expected = new List<ItemsForPurchaseDTO>
        {
            new(
                _fixture.FootballShirt.Id,
                "Football Shirt",
                "Adidas",
                39.90m,
                18,
                Gender.Female,
                "M",
                new SportForPurchaseDTO("Football"))
        };

        // Act
        var result = await GetItems(Gender.Female, null);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FiltersBySportOnly()
    {
        // Arrange
        var expected = new List<ItemsForPurchaseDTO>
        {
            new(
                _fixture.FootballShirt.Id,
                "Football Shirt",
                "Adidas",
                39.90m,
                18,
                Gender.Female,
                "M",
                new SportForPurchaseDTO("Football")),
            new(
                _fixture.FootballBoots.Id,
                "Football Boots",
                "Nike",
                59.90m,
                21,
                Gender.Male,
                "42",
                new SportForPurchaseDTO("Football"))
        };

        // Act
        var result = await GetItems(null, "Football");

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ReturnsAllAvailableItemsWhenFiltersAreMissing()
    {
        // Arrange
        var expected = new List<ItemsForPurchaseDTO>
        {
            new(
                _fixture.FootballShirt.Id,
                "Football Shirt",
                "Adidas",
                39.90m,
                18,
                Gender.Female,
                "M",
                new SportForPurchaseDTO("Football")),
            new(
                _fixture.BasketballShoes.Id,
                "Basketball Shoes",
                "Puma",
                79.90m,
                21,
                Gender.Male,
                "42",
                new SportForPurchaseDTO("Basketball")),
            new(
                _fixture.FootballBoots.Id,
                "Football Boots",
                "Nike",
                59.90m,
                21,
                Gender.Male,
                "42",
                new SportForPurchaseDTO("Football"))
        };

        // Act
        var result = await GetItems(null, null);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TreatsWhitespaceSportAsNoFilter()
    {
        // Arrange
        var expected = new List<ItemsForPurchaseDTO>
        {
            new(
                _fixture.FootballShirt.Id,
                "Football Shirt",
                "Adidas",
                39.90m,
                18,
                Gender.Female,
                "M",
                new SportForPurchaseDTO("Football")),
            new(
                _fixture.BasketballShoes.Id,
                "Basketball Shoes",
                "Puma",
                79.90m,
                21,
                Gender.Male,
                "42",
                new SportForPurchaseDTO("Basketball")),
            new(
                _fixture.FootballBoots.Id,
                "Football Boots",
                "Nike",
                59.90m,
                21,
                Gender.Male,
                "42",
                new SportForPurchaseDTO("Football"))
        };

        // Act
        var result = await GetItems(null, "   ");

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNothingMatches()
    {
        // Arrange
        var gender = Gender.Other;
        var sportName = "Tennis";

        // Act
        var result = await GetItems(gender, sportName);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
        Assert.Empty(actual);
    }

    private async Task<ActionResult<List<ItemsForPurchaseDTO>>> GetItems(
        Gender? gender,
        string? sportName)
    {
        using var context = _fixture.CreateContext();
        var controller = new ItemsController(context);

        return await controller.GetItemsForPurchase(gender, sportName);
    }
}
