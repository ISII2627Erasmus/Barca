
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppForSEII.API.Controllers;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.TeamDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AppForSEII.UT
{
    public class TeamsControllerTests
    {
        private static async Task<(ApplicationDbContext Context, Microsoft.Data.Sqlite.SqliteConnection Connection)> CreateDatabaseAsync()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var football = new Sport
            {
                Name = "Football",
                MinimumNumberOfPlayers = 2,
                NumberOfReferees = 1,
                Description = "Football sport",
                BasicRules = "Football rules"
            };

            var basketball = new Sport
            {
                Name = "Basketball",
                MinimumNumberOfPlayers = 2,
                NumberOfReferees = 1,
                Description = "Basketball sport",
                BasicRules = "Basketball rules"
            };

            var anna = new ApplicationUser(
                "captain-1", "Anna", "Smith", "anna@example.com");

            var john = new ApplicationUser(
                "captain-2", "John", "Brown", "john@example.com");

            var member = new ApplicationUser(
                "member-1", "Emma", "Jones", "emma@example.com");

            context.Sports.AddRange(football, basketball);
            context.Users.AddRange(anna, john, member);

            var alpha = new Team(
                "Alpha", 10, 18, 35, anna, football);

            var beta = new Team(
                "Beta", 10, 16, 30, john, basketball);

            var gamma = new Team(
                "Gamma", 10, 20, 40, anna, basketball);

            context.Teams.AddRange(alpha, beta, gamma);

            alpha.TeamInvitations.Add(new TeamInvitation
            {
                User = member,
                Team = alpha,
                InvitationMessage = "Please join our football team",
                InvitationAccepted = true
            });

            await context.SaveChangesAsync();

            return (context, connection);
        }

        private static TeamsController CreateController(ApplicationDbContext context)
        {
            return new TeamsController(
                context,
                NullLogger<TeamsController>.Instance);
        }

        private static List<TeamForGameDTO> GetResult(
            ActionResult<List<TeamForGameDTO>> result)
        {
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            return Assert.IsType<List<TeamForGameDTO>>(okResult.Value);
        }

        [Fact]
        public async Task GetTeamsForGame_WithoutFilters_ReturnsAllTeams()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame(null, null);
            var teams = GetResult(result);

            Assert.Equal(3, teams.Count);
            Assert.Equal(
                new[] { "Alpha", "Beta", "Gamma" },
                teams.Select(t => t.Name).ToArray());
        }

        [Fact]
        public async Task GetTeamsForGame_WithCaptainName_FiltersTeams()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame("Anna", null);
            var teams = GetResult(result);

            Assert.Equal(2, teams.Count);
            Assert.All(teams, team =>
                Assert.Equal("Anna", team.CaptainName));
        }

        [Fact]
        public async Task GetTeamsForGame_WithSportName_FiltersTeams()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame(null, "Basketball");
            var teams = GetResult(result);

            Assert.Equal(2, teams.Count);
            Assert.All(teams, team =>
                Assert.Equal("Basketball", team.SportName));
        }

        [Fact]
        public async Task GetTeamsForGame_WithBothFilters_ReturnsMatchingTeam()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame("Anna", "Basketball");
            var teams = GetResult(result);

            var team = Assert.Single(teams);
            Assert.Equal("Gamma", team.Name);
        }

        [Fact]
        public async Task GetTeamsForGame_NoMatches_ReturnsEmptyList()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame("Unknown", "Football");
            var teams = GetResult(result);

            Assert.Empty(teams);
        }

        [Fact]
        public async Task GetTeamsForGame_ReturnsCorrectDTOValues()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame("Anna", "Football");
            var teams = GetResult(result);

            var team = Assert.Single(teams);

            Assert.Equal("Alpha", team.Name);
            Assert.Equal("Anna", team.CaptainName);
            Assert.Equal("Smith", team.CaptainSurname);
            Assert.Equal("Football", team.SportName);
            Assert.Equal(2, team.NumberOfMembers);
            Assert.Equal(18, team.MinAge);
            Assert.Equal(35, team.MaxAge);
        }

        [Fact]
        public async Task GetTeamsForGame_WhitespaceFilters_ReturnsAllTeams()
        {
            var (context, connection) = await CreateDatabaseAsync();
            await using var db = context;
            await using var sqlite = connection;

            var controller = CreateController(context);

            var result = await controller.GetTeamsForGame(" ", " ");
            var teams = GetResult(result);

            Assert.Equal(3, teams.Count);
        }
    }
}


