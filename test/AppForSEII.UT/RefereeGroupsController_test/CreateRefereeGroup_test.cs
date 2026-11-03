using System.Security.Claims;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.RefereeDTOs;
using Microsoft.AspNetCore.Http;

namespace AppForSEII.UT.RefereeGroupsController_test;

public class CreateRefereeGroup_test : AppForSEII4SqliteUT
{
    private const string OwnerId = "game-owner";
    private const string OtherUserId = "other-user";
    private const int GameId = 1;
    private const int GameWithGroupId = 2;
    private const int OtherUserGameId = 3;
    private const int PastGameId = 4;

    private readonly Sport _football;
    private readonly Sport _basketball;
    private readonly Game _game;
    private readonly Referee _alex;
    private readonly Referee _sam;
    private readonly Referee _busyReferee;
    private readonly Referee _basketballReferee;
    private readonly RefereeGroup _existingGroup;

    public CreateRefereeGroup_test()
    {
        _football = new Sport
        {
            Name = "Test Football",
            MinimumNumberOfPlayers = 11,
            NumberOfReferees = 2,
            Description = "Test football",
            BasicRules = "Test rules"
        };

        _basketball = new Sport
        {
            Name = "Test Basketball",
            MinimumNumberOfPlayers = 5,
            NumberOfReferees = 1,
            Description = "Test basketball",
            BasicRules = "Test rules"
        };

        _context.Sports.AddRange(_football, _basketball);
        _context.SaveChanges();

        var owner = new ApplicationUser(
            OwnerId, "Taylor", "Manager", "manager@test.es",
            new DateOnly(1990, 1, 1), 36, Gender.Other);
        var otherUser = new ApplicationUser(
            OtherUserId, "Morgan", "Manager", "other.manager@test.es",
            new DateOnly(1990, 1, 1), 36, Gender.Other);

        _context.ApplicationUsers.AddRange(owner, otherUser);
        _context.SaveChanges();

        _game = NewGame("Test Football Game", DateTime.Today.AddDays(10), _football, owner);
        var gameWithGroup = NewGame(
            "Game With Group", DateTime.Today.AddDays(20), _football, owner);
        var otherUserGame = NewGame(
            "Other User Game", DateTime.Today.AddDays(30), _football, otherUser);
        var pastGame = NewGame(
            "Past Football Game", DateTime.Today.AddDays(-10), _football, owner);

        // The games are added in this order, so their SQLite IDs are 1 through 4.
        _context.Games.AddRange(_game, gameWithGroup, otherUserGame, pastGame);
        _context.SaveChanges();

        _alex = NewReferee("ref-alex", "Alex", "Referee", _football, 4, 3);
        _sam = NewReferee("ref-sam", "Sam", "Referee", _football, 5, 6);
        _busyReferee = NewReferee("ref-busy", "Jamie", "Busy", _football, 2, 4);
        _basketballReferee = NewReferee(
            "ref-basketball", "Casey", "Basketball", _basketball, 4, 5);

        _context.Referees.AddRange(
            _alex, _sam, _busyReferee, _basketballReferee);
        _context.SaveChanges();

        _existingGroup = new RefereeGroup(
            gameWithGroup,
            "Existing",
            "Existing group description",
            "Existing group rules");

        _context.RefereeGroups.Add(_existingGroup);
        _context.SaveChanges();

        _context.RefereeAssignments.Add(new RefereeAssignedTo(
            _existingGroup.Id,
            _busyReferee.Id,
            "Main referee",
            "Leads the referee group"));
        _context.SaveChanges();
    }

    private static Game NewGame(
        string name,
        DateTime date,
        Sport sport,
        ApplicationUser responsible)
    {
        return new Game
        {
            Name = name,
            Date = date,
            Place = "Central Stadium",
            SportId = sport.Id,
            Sport = sport,
            ResponsibleId = responsible.Id,
            Responsible = responsible
        };
    }

    private static Referee NewReferee(
        string id,
        string name,
        string surname,
        Sport sport,
        int rating,
        int yearsRefereeing)
    {
        return new Referee(
            id, name, surname, $"{id}@test.es", sport.Id, rating, yearsRefereeing)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = sport
        };
    }

    private RefereeGroupsController CreateController(
        string? userId = OwnerId,
        ApplicationDbContext? context = null)
    {
        var mock = new Mock<ILogger<RefereeGroupsController>>();
        var controller = new RefereeGroupsController(context ?? _context, mock.Object);

        var claims = userId == null
            ? Array.Empty<Claim>()
            : new[] { new Claim(ClaimTypes.NameIdentifier, userId) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Unit test"))
            }
        };

        return controller;
    }

    private static RefereeGroupDTO NewRequest(
        int gameId,
        string name = "NewGroup",
        List<RefereeAssignedToDTO>? assignments = null)
    {
        return new RefereeGroupDTO(
            gameId,
            name,
            "Group description",
            "Follow game rules",
            assignments ?? new List<RefereeAssignedToDTO>
            {
                new("ref-alex", "Main referee", "Leads the referee group"),
                new("ref-sam", "Assistant referee", "Supports the main referee")
            });
    }

    public static IEnumerable<object?[]> TestCasesFor_CreateRefereeGroup_BadRequest()
    {
        var noAssignments = NewRequest(GameId, assignments: new List<RefereeAssignedToDTO>());
        var nullAssignments = NewRequest(GameId);
        nullAssignments.RefereeAssignments = null!;

        var duplicateReferee = NewRequest(
            GameId,
            assignments: new List<RefereeAssignedToDTO>
            {
                new("ref-alex", "Main referee", "Leads the referee group"),
                new("ref-alex", "Assistant referee", "Supports the main referee")
            });

        var tooFewReferees = NewRequest(
            GameId,
            assignments: new List<RefereeAssignedToDTO>
            {
                new("ref-alex", "Main referee", "Leads the referee group")
            });

        var unknownReferee = NewRequest(
            GameId,
            assignments: new List<RefereeAssignedToDTO>
            {
                new("ref-alex", "Main referee", "Leads the referee group"),
                new("missing-referee", "Assistant referee", "Supports the main referee")
            });

        var wrongSport = NewRequest(
            GameId,
            assignments: new List<RefereeAssignedToDTO>
            {
                new("ref-alex", "Main referee", "Leads the referee group"),
                new("ref-basketball", "Assistant referee", "Supports the main referee")
            });

        var busyReferee = NewRequest(
            GameId,
            assignments: new List<RefereeAssignedToDTO>
            {
                new("ref-alex", "Main referee", "Leads the referee group"),
                new("ref-busy", "Assistant referee", "Supports the main referee")
            });

        return new List<object?[]>
        {
            new object?[] { nullAssignments, "Select at least one referee." },
            new object?[] { noAssignments, "Select at least one referee." },
            new object?[] { duplicateReferee, "A referee can only be selected once." },
            new object?[] { tooFewReferees, "This sport requires at least 2 referees." },
            new object?[] { unknownReferee, "One or more selected referees do not exist." },
            new object?[] { wrongSport, "All selected referees must be instructed in the game's sport." },
            new object?[] { busyReferee, "One or more selected referees are already assigned to a future game." }
        };
    }

    public static IEnumerable<object?[]> TestCasesFor_CreateRefereeGroup_NotFound()
    {
        return new List<object?[]>
        {
            new object?[] { NewRequest(999) },
            new object?[] { NewRequest(OtherUserGameId) },
            new object?[] { NewRequest(PastGameId) }
        };
    }

    public static IEnumerable<object?[]> TestCasesFor_CreateRefereeGroup_Conflict()
    {
        return new List<object?[]>
        {
            new object?[] { NewRequest(GameWithGroupId) },
            new object?[] { NewRequest(GameId, "Existing") }
        };
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task CreateRefereeGroup_UserIsNotAuthenticated_Unauthorized()
    {
        // Arrange
        var controller = CreateController(null);
        var request = NewRequest(GameId);

        // Act
        var result = await controller.CreateRefereeGroup(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Theory]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    [MemberData(nameof(TestCasesFor_CreateRefereeGroup_BadRequest))]
    public async Task CreateRefereeGroup_BadRequest_test(
        RefereeGroupDTO request,
        string expectedError)
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.CreateRefereeGroup(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        var actualError = problemDetails.Errors.First().Value[0];
        Assert.Equal(expectedError, actualError);
    }

    [Theory]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    [MemberData(nameof(TestCasesFor_CreateRefereeGroup_NotFound))]
    public async Task CreateRefereeGroup_NotFound_test(RefereeGroupDTO request)
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.CreateRefereeGroup(request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Theory]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    [MemberData(nameof(TestCasesFor_CreateRefereeGroup_Conflict))]
    public async Task CreateRefereeGroup_Conflict_test(RefereeGroupDTO request)
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.CreateRefereeGroup(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task CreateRefereeGroup_Success_test()
    {
        // Arrange
        var controller = CreateController();
        var request = NewRequest(GameId);

        var expected = new RefereeGroupDTO(
            _existingGroup.Id + 1,
            "NewGroup",
            "Group description",
            "Follow game rules",
            _game.Id,
            _game.Name,
            _game.Date,
            _game.Place,
            _football.Name,
            new List<RefereeAssignedToDTO>
            {
                new(
                    _alex.Id,
                    _alex.Name,
                    _alex.Surname,
                    _alex.Rating,
                    _alex.YearsRefereeing,
                    "Main referee",
                    "Leads the referee group",
                    false),
                new(
                    _sam.Id,
                    _sam.Name,
                    _sam.Surname,
                    _sam.Rating,
                    _sam.YearsRefereeing,
                    "Assistant referee",
                    "Supports the main referee",
                    false)
            });

        // Act
        var result = await controller.CreateRefereeGroup(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var actual = Assert.IsType<RefereeGroupDTO>(createdResult.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task CreateRefereeGroup_DatabaseSaveFails_Conflict()
    {
        // Arrange
        using var failingContext = new FailingSaveApplicationDbContext(_contextOptions);
        var controller = CreateController(context: failingContext);
        var request = NewRequest(GameId);

        // Act
        var result = await controller.CreateRefereeGroup(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    private sealed class FailingSaveApplicationDbContext
        : ApplicationDbContext
    {
        public FailingSaveApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Test database save failure.");
        }
    }
}
