
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using AppForSEII.API.Controllers;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.GameDTOs;
using AppForSEII.API.Models;
using System.Security.Claims;

namespace AppForSEII.UT;

public class GamesControllerCreateGameTests
{
    private const string UserId = "test-user-1";

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static GamesController CreateController(
        ApplicationDbContext context,
        bool authenticated = true)
    {
        var controller = new GamesController(
            context,
            NullLogger<GamesController>.Instance);

        var claims = authenticated
            ? new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }
            : Array.Empty<Claim>();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        claims,
                        authenticated ? "TestAuthentication" : null))
            }
        };

        return controller;
    }

    private static GameForCreateDTO ValidDTO()
    {
        return new GameForCreateDTO
        {
            Name = "Test Game",
            Date = DateTime.Now.AddDays(7),
            Place = "Albacete Stadium",
            Description = "Test description",
            GameInvitations = new List<GameInvitationDTO>
            {
                new GameInvitationDTO
                {
                    TeamId = 1,
                    Message = "Please join our game!"
                }
            }
        };
    }

    private static async Task<(ApplicationUser Captain, Sport Sport, Team Team)>
        SeedTeam(
            ApplicationDbContext context,
            string captainId = UserId,
            string teamName = "TeamOne",
            string sportName = "Football")
    {
        var captain = new ApplicationUser(
            captainId,
            "Test",
            "Captain",
            $"{captainId}@example.com");

        var sport = new Sport
        {
            Name = sportName,
            MinimumNumberOfPlayers = 2,
            NumberOfReferees = 1,
            Description = "Test sport",
            BasicRules = "Test rules"
        };

        var team = new Team(
            teamName,
            10,
            10,
            40,
            captain,
            sport);

        context.Users.Add(captain);
        context.Sports.Add(sport);
        context.Teams.Add(team);

        await context.SaveChangesAsync();

        return (captain, sport, team);
    }

    private static void AssertBadRequest(
        IActionResult result,
        string expectedMessage)
    {
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(expectedMessage, badRequest.Value);
    }

    [Fact]
    public async Task CreateGame_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        using var context = CreateContext();
        var controller = CreateController(context, false);

        var result = await controller.CreateGame(ValidDTO());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateGame_WhenNameIsEmpty_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.Name = "";

        var result = await controller.CreateGame(dto);

        AssertBadRequest(result, "Game name is required.");
    }

    [Fact]
    public async Task CreateGame_WhenDateIsInThePast_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.Date = DateTime.Now.AddDays(-1);

        var result = await controller.CreateGame(dto);

        AssertBadRequest(result, "Game date must be in the future.");
    }

    [Fact]
    public async Task CreateGame_WhenPlaceIsTooShort_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.Place = "Park";

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "Game place must contain more than 5 characters.");
    }

    [Fact]
    public async Task CreateGame_WhenInvitationsAreEmpty_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.GameInvitations = new List<GameInvitationDTO>();

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "At least one team must be selected.");
    }

    [Fact]
    public async Task CreateGame_WhenInvitationsAreNull_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.GameInvitations = null!;

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "At least one team must be selected.");
    }

    [Fact]
    public async Task CreateGame_WhenTeamIdIsInvalid_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.GameInvitations[0].TeamId = 0;

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "Each invitation must have a valid team and a message longer than 10 characters.");
    }

    [Fact]
    public async Task CreateGame_WhenInvitationMessageIsTooShort_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.GameInvitations[0].Message = "Short";

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "Each invitation must have a valid team and a message longer than 10 characters.");
    }

    [Fact]
    public async Task CreateGame_WhenGameNameAlreadyExists_ReturnsConflict()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var data = await SeedTeam(context);

        context.Games.Add(new Game
        {
            Name = "Test Game",
            Date = DateTime.Now.AddDays(3),
            Place = "Existing Stadium",
            SportId = data.Sport.Id,
            ResponsibleId = data.Captain.Id
        });

        await context.SaveChangesAsync();

        var dto = ValidDTO();
        dto.GameInvitations[0].TeamId = data.Team.Id;

        var result = await controller.CreateGame(dto);

        var conflict = Assert.IsType<ConflictObjectResult>(result);

        Assert.Equal(
            "A game with this name already exists.",
            conflict.Value);
    }

    [Fact]
    public async Task CreateGame_WhenSameTeamIsInvitedTwice_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();

        dto.GameInvitations = new List<GameInvitationDTO>
        {
            new GameInvitationDTO
            {
                TeamId = 1,
                Message = "First invitation message"
            },
            new GameInvitationDTO
            {
                TeamId = 1,
                Message = "Second invitation message"
            }
        };

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "The same team cannot be invited twice.");
    }

    [Fact]
    public async Task CreateGame_WhenSelectedTeamDoesNotExist_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var dto = ValidDTO();
        dto.GameInvitations[0].TeamId = 999;

        var result = await controller.CreateGame(dto);

        AssertBadRequest(
            result,
            "One or more selected teams do not exist.");
    }

    [Fact]
    public async Task CreateGame_WhenUserIsNotCaptain_ReturnsForbidden()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var data = await SeedTeam(
            context,
            captainId: "another-user");

        var dto = ValidDTO();
        dto.GameInvitations[0].TeamId = data.Team.Id;

        var result = await controller.CreateGame(dto);

        var forbidden = Assert.IsType<ObjectResult>(result);

        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Equal(
            "You must be the captain of at least one selected team.",
            forbidden.Value);
    }

    [Fact]
    public async Task CreateGame_WhenDataIsValid_ReturnsCreated()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var data = await SeedTeam(context);

        var dto = ValidDTO();
        dto.GameInvitations[0].TeamId = data.Team.Id;

        var result = await controller.CreateGame(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(nameof(GamesController.GetGame), created.ActionName);

        var response = Assert.IsType<GetGameDTO>(created.Value);

        Assert.Equal(dto.Name, response.Name);
        Assert.Equal(dto.Place, response.Place);
        Assert.Equal(data.Sport.Name, response.SportName);
        Assert.Single(response.InvitedTeams);

        var savedGame = await context.Games
            .Include(g => g.GameInvitations)
            .SingleAsync(g => g.Name == dto.Name);

        Assert.Equal(UserId, savedGame.ResponsibleId);
        Assert.Equal(data.Sport.Id, savedGame.SportId);
        Assert.Single(savedGame.GameInvitations);
        Assert.False(savedGame.GameInvitations[0].AcceptedGame);
    }


    
[Fact]
public async Task CreateGame_WhenTeamsHaveDifferentSports_ReturnsBadRequest()
{
    // Arrange
    using var context = CreateContext();
    var controller = CreateController(context);

    var firstTeam = await SeedTeam(
        context,
        captainId: UserId,
        teamName: "TeamOne",
        sportName: "Football");

    var secondTeam = await SeedTeam(
        context,
        captainId: "test-user-2",
        teamName: "TeamTwo",
        sportName: "Basketball");

    var dto = ValidDTO();

    dto.GameInvitations = new List<GameInvitationDTO>
    {
        new GameInvitationDTO
        {
            TeamId = firstTeam.Team.Id,
            Message = "Invitation for the first team"
        },
        new GameInvitationDTO
        {
            TeamId = secondTeam.Team.Id,
            Message = "Invitation for the second team"
        }
    };

    // Act
    var result = await controller.CreateGame(dto);

    // Assert
    AssertBadRequest(
        result,
        "All selected teams must belong to the same sport.");
}

}
