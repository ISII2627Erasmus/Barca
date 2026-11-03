using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;

namespace AppForSEII.UT.ItemsController_test;

public class GetItemsForPurchase_test : IClassFixture<Databasefixture4GetOprations>
{
    private readonly Databasefixture4GetOprations _fixture;

    public GetItemsForPurchase_test(Databasefixture4GetOprations fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FiltersByGenderAndSport()
    {
        var items = await GetItems(Gender.Male, "Football");

        var item = Assert.Single(items);
        Assert.Equal("Football Boots", item.Name);
        Assert.Equal("Nike", item.Brand);
        Assert.Equal(59.90m, item.Price);
        Assert.Equal("Football", item.Sport.Name);
    }

    [Fact]
    public async Task FiltersByGenderOnly()
    {
        var items = await GetItems(Gender.Female, null);

        Assert.Equal("Football Shirt", Assert.Single(items).Name);
    }

    [Fact]
    public async Task FiltersBySportOnly()
    {
        var items = await GetItems(null, "Football");

        Assert.Equal(
            new[] { "Football Shirt", "Football Boots" },
            items.Select(item => item.Name));
    }

    [Fact]
    public async Task ReturnsAllAvailableItemsWhenFiltersAreMissing()
    {
        var items = await GetItems(null, null);

        Assert.Equal(
            new[] { "Football Shirt", "Basketball Shoes", "Football Boots" },
            items.Select(item => item.Name));
    }

    [Fact]
    public async Task TreatsWhitespaceSportAsNoFilter()
    {
        var items = await GetItems(null, "   ");

        Assert.Equal(
            new[] { "Football Shirt", "Basketball Shoes", "Football Boots" },
            items.Select(item => item.Name));
    }

    [Fact]
    public async Task ReturnsEmptyListWhenNothingMatches()
    {
        var items = await GetItems(Gender.Other, "Tennis");

        Assert.Empty(items);
    }

    private async Task<List<ItemsForPurchaseDTO>> GetItems(
        Gender? gender,
        string? sportName)
    {
        using var context = _fixture.CreateContext();
        var result = await new ItemsController(context)
            .GetItemsForPurchase(gender, sportName);
        var response = Assert.IsType<OkObjectResult>(result.Result);

        return Assert.IsType<List<ItemsForPurchaseDTO>>(response.Value);
    }
}
