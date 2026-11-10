namespace AppForSEII.UT;

public sealed class DatabaseFixture4GetOperations : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

    public Item FootballBoots { get; }
    public Item FootballShirt { get; }
    public Item BasketballShoes { get; }
    public ApplicationUser Customer { get; }
    public Purchase ExistingPurchase { get; }

    public DatabaseFixture4GetOperations()
    {
        // Keep this connection open for the fixture lifetime so the SQLite in-memory
        // database is shared by every context created for the tests in this class.
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();

        var football = new Sport
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

        context.Sports.AddRange(football, basketball);
        context.SaveChanges();

        FootballBoots = new Item(
            "Football Boots", "Nike", 59.90m, 21, Gender.Male, "42", 5, football);
        FootballShirt = new Item(
            "Football Shirt", "Adidas", 39.90m, 18, Gender.Female, "M", 8, football);
        BasketballShoes = new Item(
            "Basketball Shoes", "Puma", 79.90m, 21, Gender.Male, "42", 4, basketball);
        var outOfStockItem = new Item(
            "Out of stock", "Nike", 29.90m, 21, Gender.Male, "42", 0, football);

        context.Items.AddRange(FootballBoots, FootballShirt, BasketballShoes, outOfStockItem);

        Customer = new ApplicationUser("purchase-user", "Taylor", "Smith", "taylor@example.com")
        {
            BirthDate = new DateOnly(2000, 1, 1),
            Age = 26,
            Gender = Gender.Male
        };
        context.ApplicationUsers.Add(Customer);
        context.SaveChanges();

        ExistingPurchase = new Purchase(
            Customer,
            "Main Street 1",
            "Barcelona",
            "Barcelona",
            "4111111111111111",
            "123",
            "12/30",
            "Fixture purchase")
        {
            PurchaseDate = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        };
        ExistingPurchase.PurchaseItems.Add(
            new PurchaseItem(ExistingPurchase, FootballBoots, quantity: 2));
        ExistingPurchase.TotalPrice = FootballBoots.Price * 2;
        FootballBoots.QuantityAvailableForPurchase -= 2;

        context.Purchases.Add(ExistingPurchase);
        context.SaveChanges();
    }

    public ApplicationDbContext CreateContext() => new(_contextOptions);

    public void Dispose() => _connection.Dispose();
}
