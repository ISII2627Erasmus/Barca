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

<<<<<<< HEAD
        var item = Assert.Single(items);
        Assert.Equal("Football Boots", item.Name);
        Assert.Equal("Nike", item.Brand);
        Assert.Equal(59.90m, item.Price);
        Assert.Equal("Football", item.Sport.Name);
=======
        Assert.Equal(
            new[] { ExpectedItem(_fixture.FootballBoots) },
            items);
>>>>>>> development
    }

    [Fact]
    public async Task FiltersByGenderOnly()
    {
        var items = await GetItems(Gender.Female, null);

<<<<<<< HEAD
        Assert.Equal("Football Shirt", Assert.Single(items).Name);
=======
        Assert.Equal(
            new[] { ExpectedItem(_fixture.FootballShirt) },
            items);
>>>>>>> development
    }

    [Fact]
    public async Task FiltersBySportOnly()
    {
        var items = await GetItems(null, "Football");

        Assert.Equal(
<<<<<<< HEAD
            new[] { "Football Shirt", "Football Boots" },
            items.Select(item => item.Name));
=======
            new[]
            {
                ExpectedItem(_fixture.FootballShirt),
                ExpectedItem(_fixture.FootballBoots)
            },
            items);
>>>>>>> development
    }

    [Fact]
    public async Task ReturnsAllAvailableItemsWhenFiltersAreMissing()
    {
        var items = await GetItems(null, null);

        Assert.Equal(
<<<<<<< HEAD
            new[] { "Football Shirt", "Basketball Shoes", "Football Boots" },
            items.Select(item => item.Name));
=======
            new[]
            {
                ExpectedItem(_fixture.FootballShirt),
                ExpectedItem(_fixture.BasketballShoes),
                ExpectedItem(_fixture.FootballBoots)
            },
            items);
>>>>>>> development
    }

    [Fact]
    public async Task TreatsWhitespaceSportAsNoFilter()
    {
        var items = await GetItems(null, "   ");

        Assert.Equal(
<<<<<<< HEAD
            new[] { "Football Shirt", "Basketball Shoes", "Football Boots" },
            items.Select(item => item.Name));
=======
            new[]
            {
                ExpectedItem(_fixture.FootballShirt),
                ExpectedItem(_fixture.BasketballShoes),
                ExpectedItem(_fixture.FootballBoots)
            },
            items);
>>>>>>> development
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
<<<<<<< HEAD
=======

    private static ItemsForPurchaseDTO ExpectedItem(Item item)
    {
        return new ItemsForPurchaseDTO(
            item.Id,
            item.Name,
            item.Brand,
            item.Price,
            item.RecommendedAge,
            item.Gender,
            item.Size,
            new SportForPurchaseDTO(item.Sport.Id, item.Sport.Name));
    }
>>>>>>> development
}
