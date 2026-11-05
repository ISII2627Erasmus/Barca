
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

            var sport = new Sport(
                "Football",
                11,
                2,
                1,
                "Football sport",
                "Standard football rules"
            );

            var responsible = new ApplicationUser(
                "user-1",
                "Test",
                "User",
                "testuser@example.com"
            )
            {
                BirthDate = new DateOnly(2000, 1, 1),
                Age = 26
            };

            var game = new Game(
                "Friendly Match",
                new DateTime(2026, 11, 10),
                "Albacete Stadium",
                "Friendly football match",
                sport,
                responsible
            );

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
            var actualDTO = Assert.IsType<GetGameDTO>(okResult.Value);

            var expectedDTO = new GetGameDTO
            {
                Id = game.Id,
                Name = "Friendly Match",
                Date = new DateTime(2026, 11, 10),
                Place = "Albacete Stadium",
                Description = "Friendly football match",
                SportName = "Football",
                InvitedTeams = new List<InvitedTeamDTO>()
            };

            Assert.True(expectedDTO.Equals(actualDTO));
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

            var sport = new Sport(
                "Basketball",
                5,
                2,
                2,
                "Basketball sport",
                "Standard basketball rules"
            );

            var responsible = new ApplicationUser(
                "responsible-1",
                "Test",
                "Organizer",
                "organizer@example.com"
            )
            {
                BirthDate = new DateOnly(1998, 1, 1),
                Age = 28
            };

            var captain = new ApplicationUser(
                "captain-1",
                "John",
                "Smith",
                "captain@example.com"
            )
            {
                BirthDate = new DateOnly(2000, 1, 1),
                Age = 26
            };

            var member1 = new ApplicationUser(
                "member-1",
                "Alice",
                "Brown",
                "alice@example.com"
            )
            {
                BirthDate = new DateOnly(2002, 1, 1),
                Age = 24
            };

            var member2 = new ApplicationUser(
                "member-2",
                "Bob",
                "Wilson",
                "bob@example.com"
            )
            {
                BirthDate = new DateOnly(2001, 1, 1),
                Age = 25
            };

            var member3 = new ApplicationUser(
                "member-3",
                "Emma",
                "Davis",
                "emma@example.com"
            )
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

            var game = new Game(
                "Basketball Match",
                new DateTime(2026, 11, 15),
                "Albacete Sports Center",
                "Friendly basketball game",
                sport,
                responsible
            );

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
            var actualDTO = Assert.IsType<GetGameDTO>(okResult.Value);

            var expectedDTO = new GetGameDTO
            {
                Id = game.Id,
                Name = "Basketball Match",
                Date = new DateTime(2026, 11, 15),
                Place = "Albacete Sports Center",
                Description = "Friendly basketball game",
                SportName = "Basketball",
                InvitedTeams = new List<InvitedTeamDTO>
                {
                    new InvitedTeamDTO
                    {
                        TeamId = team.Id,
                        TeamName = "Tigers",
                        CaptainName = "John",
                        CaptainSurname = "Smith",
                        NumberOfMembers = 3,
                        MinAge = 18,
                        MaxAge = 35,
                        SportName = "Basketball",
                        Message = "You are invited to our basketball match"
                    }
                }
            };

            Assert.True(expectedDTO.Equals(actualDTO));
        }
    }
}
