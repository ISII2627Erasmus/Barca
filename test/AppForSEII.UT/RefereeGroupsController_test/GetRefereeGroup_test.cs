using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.RefereeDTOs;

namespace AppForSEII.UT.RefereeGroupsController_test;

public class GetRefereeGroup_test : AppForSEII4SqliteUT
{
    private readonly RefereeGroup _group;
    private readonly Referee _alex;
    private readonly Referee _sam;
    private readonly Game _game;
    private readonly Sport _football;

    public GetRefereeGroup_test()
    {
        _football = new Sport
        {
            Name = "Test Football",
            MinimumNumberOfPlayers = 11,
            NumberOfReferees = 2,
            Description = "Test football",
            BasicRules = "Test rules"
        };

        _context.Sports.Add(_football);
        _context.SaveChanges();

        var responsible = new ApplicationUser(
            "game-owner",
            "Taylor",
            "Manager",
            "manager@test.es",
            new DateOnly(1990, 1, 1),
            36,
            Gender.Other);

        _context.ApplicationUsers.Add(responsible);
        _context.SaveChanges();

        _game = new Game
        {
            Name = "Test Football Game",
            Date = DateTime.Today.AddDays(10),
            Place = "Central Stadium",
            SportId = _football.Id,
            Sport = _football,
            ResponsibleId = responsible.Id,
            Responsible = responsible
        };

        _context.Games.Add(_game);
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

        _sam = new Referee(
            "ref-sam", "Sam", "Referee", "sam.referee@test.es",
            _football.Id, 5, 6)
        {
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 31,
            Gender = Gender.Other,
            Sport = _football
        };

        _context.Referees.AddRange(_sam, _alex);
        _context.SaveChanges();

        _group = new RefereeGroup(
            _game,
            "RefGroup01",
            "Test group description",
            "Test group rules");

        _context.RefereeGroups.Add(_group);
        _context.SaveChanges();

        _context.RefereeAssignments.AddRange(
            new RefereeAssignedTo(
                _group.Id,
                _sam.Id,
                "Assistant referee",
                "Supports the main referee"),
            new RefereeAssignedTo(
                _group.Id,
                _alex.Id,
                "Main referee",
                "Leads the referee group"));
        _context.SaveChanges();
    }

    private RefereeGroupsController CreateController()
    {
        return new RefereeGroupsController(
            _context,
            new Mock<ILogger<RefereeGroupsController>>().Object);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereeGroup_IdIsNotPositive_BadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetRefereeGroup(0);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<ValidationProblemDetails>(badRequest.Value);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereeGroup_GroupDoesNotExist_NotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetRefereeGroup(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    [Trait("LevelTesting", "Unit Testing")]
    [Trait("Database", "WithoutFixture")]
    public async Task GetRefereeGroup_GroupExists_ReturnsGroupAndAssignments()
    {
        // Arrange
        var controller = CreateController();

        var expected = new RefereeGroupDTO(
            _group.Id,
            _group.Name,
            _group.Description,
            _group.Rules,
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
        var result = await controller.GetRefereeGroup(_group.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsType<RefereeGroupDTO>(okResult.Value);
        Assert.Equal(expected, actual);
    }
}
