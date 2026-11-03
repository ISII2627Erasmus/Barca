using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.TeamDTOs;

namespace AppForSEII.UT.TeamsController_test
{
    public class GetTeam_test : AppForSEII4SqliteUT
    {
        public GetTeam_test()
        {
            var sport = new Sport { Name = "Futsal", MinimumNumberOfPlayers = 3, NumberOfReferees = 1, Description = "Futsal description", BasicRules = "Futsal rules" };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser("1", "Petra", "Kovac", "petra@uclm.es", new DateOnly(2000, 1, 1), 26, Gender.Female),
                new ApplicationUser("2", "Peter", "Jackson", "peter@uclm.es", new DateOnly(1996, 1, 1), 30, Gender.Male),
                new ApplicationUser("3", "Ivan", "Novak", "ivan@uclm.es", new DateOnly(2002, 1, 1), 24, Gender.Male),
            };

            var team = new Team("Lions", 5, 18, 40, users[0], sport, "Futsal team");

            _context.Add(sport);
            _context.AddRange(users);
            _context.Add(team);
            _context.SaveChanges();

            var invitations = new List<TeamInvitation>()
            {
                new TeamInvitation("2", team.Id, "Join our team!"),
                new TeamInvitation("3", team.Id, "We need you!"),
            };

            _context.AddRange(invitations);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit testing")]
        public async Task GetTeam_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<TeamsController>>();
            ILogger<TeamsController> logger = mock.Object;
            var controller = new TeamsController(_context, logger);

            // Act
            var result = await controller.GetTeam(0);

            // Assert
            //we check that the response type is NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetTeam_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<TeamsController>>();
            ILogger<TeamsController> logger = mock.Object;
            var controller = new TeamsController(_context, logger);

            var expectedTeam = new TeamDetailDTO(1, "Lions", "Futsal", 18, 40, "Petra", "Kovac", 26,
                new List<TeamInvitationDTO>
                {
                    new TeamInvitationDTO("2", "Peter", "Jackson", 30, "Join our team!"),
                    new TeamInvitationDTO("3", "Ivan", "Novak", 24, "We need you!"),
                });

            // Act
            var result = await controller.GetTeam(1);

            // Assert
            //we check that the response type is OK and obtain the team
            var okResult = Assert.IsType<OkObjectResult>(result);
            var teamActual = Assert.IsType<TeamDetailDTO>(okResult.Value);
            Assert.Equal(expectedTeam, teamActual);
        }
    }
}
