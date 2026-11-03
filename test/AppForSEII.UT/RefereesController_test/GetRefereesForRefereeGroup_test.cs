using System.Security.Claims;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.RefereeDTOs;
using Microsoft.AspNetCore.Http;

namespace AppForSEII.UT.RefereesController_test;

public class GetRefereesForRefereeGroup_test : AppForSEII4SqliteUT
{
    private readonly Sport _football;
    private readonly Game _gameOne;
    private readonly Game _gameTwo;
    private readonly Referee _alex;
    private readonly Referee _morgan;
    private readonly Referee _sam;

    private const string ResponsibleId = "responsible-user";

    public GetRefereesForRefereeGroup_test()
    {
        _football = new Sport
        {
            Name = "Test Football",
            MinimumNumberOfPlayers = 11,
            NumberOfReferees = 2,
            Description = "Test football",
            BasicRules = "Test rules"
        };

        var basketball = new Sport
        {
            Name = "Test Basketball",
            MinimumNumberOfPlayers = 5,
            NumberOfReferees = 2,
            Description = "Test basketball",
            BasicRules = "Test rules"
        };

        _context.Sports.AddRange(_football, basketball);
        _context.SaveChanges();

        var responsible = new ApplicationUser(
            ResponsibleId,
            "Alex",
            "Manager",
            "manager@test.es",
            new DateOnly(1990, 1, 1),
            36,
            Gender.Other);

        _context.ApplicationUsers.Add(responsible);
        _context.SaveChanges();

        _gameOne = new Game
        {
            Name = "Test Football Game One",
            Date = DateTime.Today.AddDays(10),
            Place = "Central Stadium",
            SportId = _football.Id,
            Sport = _football,
            ResponsibleId = responsible.Id,
            Responsible = responsible
        };

        _gameTwo = new Game
        {
            Name = "Test Football Game Two",
            Date = DateTime.Today.AddDays(20),
            Place = "Central Stadium",
            SportId = _football.Id,
            Sport = _football,
            ResponsibleId = responsible.Id,
            Responsible = responsible
        };

        var gameWithGroup = new Game
        {
            Name = "Test Football Game With Group",
            Date = DateTime.Today.AddDays(30),
            Place = "Central Stadium",
            SportId = _football.Id,
            Sport = _football,
            ResponsibleId = responsible.Id,
            Responsible = responsible
        };

        var pastGame = new Game
        {
            Name = "Test Past Football Game",
            Date = DateTime.Today.AddDays(-10),
            Place = "Central Stadium",
            SportId = _football.Id,
            Sport = _football,
            ResponsibleId = responsible.Id,
            Responsible = responsible
        };

        _context.Games.AddRange(_gameOne, _gameTwo, gameWithGroup, pastGame);
        _context.SaveChanges();

        _alex = new Referee(
            "ref-alex", "Alex", "Referee", "alex.referee@test.es",
            _football.Id, 4, 3)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = _football
        };

        _morgan = new Referee(
            "ref-morgan", "Morgan", "Referee", "morgan.referee@test.es",
            _football.Id, 3, 1)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = _football
        };

        _sam = new Referee(
            "ref-sam", "Sam", "Referee", "sam.referee@test.es",
            _football.Id, 5, 6)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = _football
        };

        var busyReferee = new Referee(
            "ref-busy", "Jamie", "Busy", "jamie.busy@test.es",
            _football.Id, 2, 4)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = _football
        };

        var basketballReferee = new Referee(
            "ref-basketball", "Casey", "Basketball", "casey.basketball@test.es",
            basketball.Id, 4, 5)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = basketball
        };

        var refereeWithoutExperience = new Referee(
            "ref-no-experience", "Taylor", "Referee", "taylor.referee@test.es",
            _football.Id, 3, 0)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = _football
        };

        _context.Referees.AddRange(
            _alex, _morgan, _sam, busyReferee, basketballReferee,
            refereeWithoutExperience);
        _context.SaveChanges();

        var refereeGroup = new RefereeGroup(
            gameWithGroup,
            "BusyGroup",
            "Test group description",
            "Test group rules");

        _context.RefereeGroups.Add(refereeGroup);
        _context.SaveChanges();

        _context.RefereeAssignments.Add(new RefereeAssignedTo(
            refereeGroup.Id,
            busyReferee.Id,
            "Main referee",
            "Leads the referee group"));
        _context.SaveChanges();
    }

    private RefereesController CreateController(string? userId = ResponsibleId)
    {
        var controller = new RefereesController(
            _context,
            new Mock<ILogger<RefereesController>>().Object);

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

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_MinimumYearsIsBelowOne_BadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetRefereesForRefereeGroup(
            minYearsRefereeing: 0);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_UserIsNotAuthenticated_Unauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.GetRefereesForRefereeGroup();

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_UserHasNoEligibleGame_NotFound()
    {
        // Arrange
        var controller = CreateController("user-with-no-games");

        // Act
        var result = await controller.GetRefereesForRefereeGroup();

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_NoFilters_ReturnsLatestGameAndAvailableReferees()
    {
        // Arrange
        var controller = CreateController();

        var expected = new RefereesForRefereeGroupDTO(
            _gameTwo.Id,
            _gameTwo.Name,
            _gameTwo.Date,
            _gameTwo.Place,
            _football.Name,
            _football.NumberOfReferees,
            new List<RefereeForRefereeGroupDTO>
            {
                new(_alex.Id, _alex.Name, _alex.Surname, _alex.Rating, _alex.YearsRefereeing),
                new(_morgan.Id, _morgan.Name, _morgan.Surname, _morgan.Rating, _morgan.YearsRefereeing),
                new(_sam.Id, _sam.Name, _sam.Surname, _sam.Rating, _sam.YearsRefereeing)
            });

        // Act
        var result = await controller.GetRefereesForRefereeGroup();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<RefereesForRefereeGroupDTO>(okResult.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_GameAndMinimumYearsProvided_ReturnsMatchingReferees()
    {
        // Arrange
        var controller = CreateController();

        var expected = new RefereesForRefereeGroupDTO(
            _gameOne.Id,
            _gameOne.Name,
            _gameOne.Date,
            _gameOne.Place,
            _football.Name,
            _football.NumberOfReferees,
            new List<RefereeForRefereeGroupDTO>
            {
                new(_sam.Id, _sam.Name, _sam.Surname, _sam.Rating, _sam.YearsRefereeing)
            });

        // Act
        var result = await controller.GetRefereesForRefereeGroup(
            _gameOne.Id,
            minYearsRefereeing: 4);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<RefereesForRefereeGroupDTO>(okResult.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_MinimumYearsIsOne_ReturnsAllAvailableReferees()
    {
        // Arrange
        var controller = CreateController();

        var expected = new RefereesForRefereeGroupDTO(
            _gameTwo.Id,
            _gameTwo.Name,
            _gameTwo.Date,
            _gameTwo.Place,
            _football.Name,
            _football.NumberOfReferees,
            new List<RefereeForRefereeGroupDTO>
            {
                new(_alex.Id, _alex.Name, _alex.Surname, _alex.Rating, _alex.YearsRefereeing),
                new(_morgan.Id, _morgan.Name, _morgan.Surname, _morgan.Rating, _morgan.YearsRefereeing),
                new(_sam.Id, _sam.Name, _sam.Surname, _sam.Rating, _sam.YearsRefereeing)
            });

        // Act
        var result = await controller.GetRefereesForRefereeGroup(
            minYearsRefereeing: 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<RefereesForRefereeGroupDTO>(okResult.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereesForRefereeGroup_NoRefereesMeetMinimumYears_ReturnsEmptyList()
    {
        // Arrange
        var controller = CreateController();

        var expected = new RefereesForRefereeGroupDTO(
            _gameTwo.Id,
            _gameTwo.Name,
            _gameTwo.Date,
            _gameTwo.Place,
            _football.Name,
            _football.NumberOfReferees,
            new List<RefereeForRefereeGroupDTO>());

        // Act
        var result = await controller.GetRefereesForRefereeGroup(
            _gameTwo.Id,
            minYearsRefereeing: 7);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<RefereesForRefereeGroupDTO>(okResult.Value);
        Assert.Equal(expected, actual);
    }
}
