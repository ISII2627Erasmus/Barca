
using AppForSEII.API.Controllers;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.GameDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AppForSEII.UT
{
    public class GamesControllerTests
    {
        // TC1: Game does not exist -> 404 NotFound
        [Fact]
        public async Task GetGame_WhenGameDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var controller = new GamesController(
                context,
                NullLogger<GamesController>.Instance
            );

            // Act
            var result = await controller.GetGame(-1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // TC2: Game exists -> 200 OK with correct GetGameDTO
        [Fact]
        public async Task GetGame_WhenGameExists_ReturnsOkWithCorrectGameDTO()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var sport = new Sport
            {
                Name = "Football",
                MinimumNumberOfPlayers = 11,
                NumberOfReferees = 1,
                Description = "Football sport",
                BasicRules = "Standard football rules"
            };

            var responsible = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Name = "Test",
                Surname = "User",
                BirthDate = new DateOnly(2000, 1, 1),
                Age = 26
            };

            var game = new Game
            {
                Name = "Friendly Match",
                Date = new DateTime(2026, 11, 10),
                Place = "Albacete Stadium",
                Description = "Friendly football match",
                Sport = sport,
                Responsible = responsible,
                ResponsibleId = responsible.Id
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            var controller = new GamesController(
                context,
                NullLogger<GamesController>.Instance
            );

            // Act
            var result = await controller.GetGame(game.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var gameDTO = Assert.IsType<GetGameDTO>(okResult.Value);

            Assert.Equal(game.Id, gameDTO.Id);
            Assert.Equal("Friendly Match", gameDTO.Name);
            Assert.Equal(new DateTime(2026, 11, 10), gameDTO.Date);
            Assert.Equal("Albacete Stadium", gameDTO.Place);
            Assert.Equal("Friendly football match", gameDTO.Description);
            Assert.Equal("Football", gameDTO.SportName);
            Assert.Empty(gameDTO.InvitedTeams);
        }

        // TC3: Game has invited teams -> correct team details and member count
        [Fact]
        public async Task GetGame_WhenGameHasInvitedTeams_ReturnsCorrectTeamDetails()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var sport = new Sport
            {
                Name = "Basketball",
                MinimumNumberOfPlayers = 5,
                NumberOfReferees = 2,
                Description = "Basketball sport",
                BasicRules = "Standard basketball rules"
            };

            var responsible = new ApplicationUser(
                "responsible-1", "Test", "Organizer",
                "organizer@example.com")
            {
                BirthDate = new DateOnly(1998, 1, 1),
                Age = 28
            };

            var captain = new ApplicationUser(
                "captain-1", "John", "Smith",
                "captain@example.com")
            {
                BirthDate = new DateOnly(2000, 1, 1),
                Age = 26
            };

            var member1 = new ApplicationUser(
                "member-1", "Alice", "Brown",
                "alice@example.com")
            {
                BirthDate = new DateOnly(2002, 1, 1),
                Age = 24
            };

            var member2 = new ApplicationUser(
                "member-2", "Bob", "Wilson",
                "bob@example.com")
            {
                BirthDate = new DateOnly(2001, 1, 1),
                Age = 25
            };

            var member3 = new ApplicationUser(
                "member-3", "Emma", "Davis",
                "emma@example.com")
            {
                BirthDate = new DateOnly(2003, 1, 1),
                Age = 23
            };

            var team = new Team
            {
                Name = "Tigers",
                MaxMembers = 10,
                MinAge = 18,
                MaxAge = 35,
                Captain = captain,
                Sport = sport
            };

            team.TeamInvitations.Add(new TeamInvitation
            {
                User = member1,
                Team = team,
                InvitationMessage = "Join our basketball team",
                InvitationAccepted = true
            });

            team.TeamInvitations.Add(new TeamInvitation
            {
                User = member2,
                Team = team,
                InvitationMessage = "Join our basketball team",
                InvitationAccepted = true
            });

            team.TeamInvitations.Add(new TeamInvitation
            {
                User = member3,
                Team = team,
                InvitationMessage = "Join our basketball team",
                InvitationAccepted = false
            });

            var game = new Game
            {
                Name = "Basketball Match",
                Date = new DateTime(2026, 11, 15),
                Place = "Albacete Sports Center",
                Description = "Friendly basketball game",
                Sport = sport,
                Responsible = responsible,
                ResponsibleId = responsible.Id
            };

            var invitation = new GameInvitation
            {
                Game = game,
                Team = team,
                Message = "You are invited to our basketball match",
                AcceptedGame = false
            };

            game.GameInvitations.Add(invitation);

            context.Games.Add(game);
            await context.SaveChangesAsync();

            var controller = new GamesController(
                context,
                NullLogger<GamesController>.Instance
            );

            // Act
            var result = await controller.GetGame(game.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var gameDTO = Assert.IsType<GetGameDTO>(okResult.Value);

            Assert.Equal(game.Id, gameDTO.Id);
            Assert.Equal("Basketball Match", gameDTO.Name);
            Assert.Equal("Basketball", gameDTO.SportName);

            var invitedTeam = Assert.Single(gameDTO.InvitedTeams);

            Assert.Equal(team.Id, invitedTeam.TeamId);
            Assert.Equal("Tigers", invitedTeam.TeamName);
            Assert.Equal("John", invitedTeam.CaptainName);
            Assert.Equal("Smith", invitedTeam.CaptainSurname);
            Assert.Equal(3, invitedTeam.NumberOfMembers);
            Assert.Equal(18, invitedTeam.MinAge);
            Assert.Equal(35, invitedTeam.MaxAge);
            Assert.Equal("Basketball", invitedTeam.SportName);
            Assert.Equal(
                "You are invited to our basketball match",
                invitedTeam.Message
            );
        }
    }
}
