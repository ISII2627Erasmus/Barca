using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;

namespace AppForSEII.UT.ItemsController_test;

public class GetItemsForPurchase_test : AppForSEII4SqliteUT
{
    private readonly Sport _football;
    private readonly Item _footballBoots;

    public GetItemsForPurchase_test()
    {
        _football = new Sport
        {
            Name = "Football",
            MinimumNumberOfPlayers = 11,
            NumberOfReferees = 1,
            Description = "Football",
            BasicRules = "Score goals"
        };
        var basketball = new Sport
        {
            Name = "Basketball",
            MinimumNumberOfPlayers = 5,
            NumberOfReferees = 2,
            Description = "Basketball",
            BasicRules = "Score points"
        };

        _context.Sports.AddRange(_football, basketball);
        _context.SaveChanges();

        _footballBoots = new Item(
            "Football Boots", "Nike", 59.90m, 21, Gender.Male, "42", 5, _football);
        _context.Items.AddRange(
            _footballBoots,
            new Item("Football Shirt", "Adidas", 39.90m, 18, Gender.Female, "M", 8, _football),
            new Item("Basketball Shoes", "Puma", 79.90m, 21, Gender.Male, "42", 4, basketball),
            new Item("Out of stock", "Nike", 29.90m, 21, Gender.Male, "42", 0, _football));
        _context.SaveChanges();
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetItemsForPurchase_Found_test()
    {
        // Arrange
        var controller = new ItemsController(_context);

        // Act
        var result = await controller.GetItemsForPurchase(
            Gender.Male,
            "Football",
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<ItemsForPurchaseDTO>>(okResult.Value);
        var item = Assert.Single(items);
        Assert.Equal("Football Boots", item.Name);
        Assert.Equal("Nike", item.Brand);
        Assert.Equal(59.90m, item.Price);
        Assert.Equal(21, item.RecommendedAge);
        Assert.Equal(Gender.Male, item.Gender);
        Assert.Equal("42", item.Size);
        Assert.Equal(_football.Id, item.Sport.Id);
        Assert.Equal("Football", item.Sport.Name);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetItemsForPurchase_NoMatches_ReturnsEmptyList_test()
    {
        // Arrange
        var controller = new ItemsController(_context);

        // Act
        var result = await controller.GetItemsForPurchase(
            Gender.Male,
            "Tennis",
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<ItemsForPurchaseDTO>>(okResult.Value);
        Assert.Empty(items);
    }
}
