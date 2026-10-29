using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.TeamDTOs;

namespace AppForSEII.UT.TeamsController_test
{
    public class CreateTeam_test : AppForSEII4SqliteUT
    {
        public CreateTeam_test()
        {
            var sport = new Sport { Name = "Futsal", MinimumNumberOfPlayers = 3, NumberOfReferees = 1, Description = "Futsal description", BasicRules = "Futsal rules" };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser("1", "Petra", "Kovac", "petra@uclm.es", new DateOnly(2000, 1, 1), 26, Gender.Female),
                new ApplicationUser("2", "Peter", "Jackson", "peter@uclm.es", new DateOnly(1996, 1, 1), 30, Gender.Male),
                new ApplicationUser("3", "Ivan", "Novak", "ivan@uclm.es", new DateOnly(2002, 1, 1), 24, Gender.Male),
            };

            //this team already exists, so its name cannot be chosen again
            var team = new Team("Lions", 5, 18, 40, users[0], sport, "Futsal team");

            _context.Add(sport);
            _context.AddRange(users);
            _context.Add(team);
            _context.SaveChanges();
        }

        public static IEnumerable<object[]> TestCasesFor_CreateTeam_BadRequest()
        {
            var peter = new TeamInvitationDTO("2", "Peter", "Jackson", 30, "Join our team!");
            var ivan = new TeamInvitationDTO("3", "Ivan", "Novak", 24, "Join our team!");
            var unknown = new TeamInvitationDTO("99", "John", "Doe", 20, "Join our team!");
            var captain = new TeamInvitationDTO("1", "Petra", "Kovac", 26, "Join our team!");

            var teamMinAgeGreaterThanMaxAge = new TeamForCreateDTO("Tigers", 5, 40, 18, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter, ivan });
            var teamWithoutInvitations = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO>());
            var teamExceedsMaxMembers = new TeamForCreateDTO("Tigers", 2, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter, ivan });
            var teamCaptainNotExists = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "nobody@uclm.es",
                new List<TeamInvitationDTO> { peter, ivan });
            var teamSportNotExists = new TeamForCreateDTO("Tigers", 5, 18, 40, "Tennis", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter, ivan });
            var teamTooFewPlayers = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter });
            var teamDuplicateInvitations = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter, peter });
            var teamUserNotExists = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter, unknown });
            var teamCaptainInvited = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO> { peter, captain });

            var allTests = new List<object[]>
            {   //team to create, expected error
                new object[] { teamMinAgeGreaterThanMaxAge, "Error! The minimum age must be lower than or equal to the maximum age" },
                new object[] { teamWithoutInvitations, "Error! The team must have at least one invitation" },
                new object[] { teamExceedsMaxMembers, "Error! The maximum number of members must be greater than or equal to the number of invitations + 1 (captain)" },
                new object[] { teamCaptainNotExists, "Error! The captain with username nobody@uclm.es does not exist" },
                new object[] { teamSportNotExists, "Error! The sport with name Tennis does not exist" },
                new object[] { teamTooFewPlayers, "Error! Futsal needs at least 3 players, please select additional teammates" },
                new object[] { teamDuplicateInvitations, "Error! There are duplicate invitations" },
                new object[] { teamUserNotExists, "Error! The player with id '99' does not exist" },
                new object[] { teamCaptainInvited, "Error! The captain cannot be invited to the team" },
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_CreateTeam_BadRequest))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]

        public async Task CreateTeam_BadRequest_test(TeamForCreateDTO teamForCreate, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<TeamsController>>();
            ILogger<TeamsController> logger = mock.Object;
            var controller = new TeamsController(_context, logger);

            // Act
            var result = await controller.CreateTeam(teamForCreate);

            // Assert
            //we check that the response type is BadRequest and obtain the error returned
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.Equal(errorExpected, errorActual);
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateTeam_Conflict_test()
        {
            // Arrange
            var mock = new Mock<ILogger<TeamsController>>();
            ILogger<TeamsController> logger = mock.Object;
            var controller = new TeamsController(_context, logger);

            var teamForCreate = new TeamForCreateDTO("Lions", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO>
                {
                    new TeamInvitationDTO("2", "Peter", "Jackson", 30, "Join our team!"),
                    new TeamInvitationDTO("3", "Ivan", "Novak", 24, "Join our team!"),
                });

            // Act
            var result = await controller.CreateTeam(teamForCreate);

            // Assert
            //we check that the response type is Conflict and obtain the error returned
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("A team with the name 'Lions' already exists.", conflictResult.Value);
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateTeam_Created_test()
        {
            var mock = new Mock<ILogger<TeamsController>>();
            ILogger<TeamsController> logger = mock.Object;
            var controller = new TeamsController(_context, logger);

            var teamForCreate = new TeamForCreateDTO("Tigers", 5, 18, 40, "Futsal", "petra@uclm.es",
                new List<TeamInvitationDTO>
                {
                    new TeamInvitationDTO("2", "Peter", "Jackson", 30, "Join our team!"),
                    new TeamInvitationDTO("3", "Ivan", "Novak", 24, "We need you!"),
                }, "New futsal team");

            //the new team gets id 2 because Lions already has id 1
            var expectedTeam = new TeamDetailDTO(2, "Tigers", "Futsal", 18, 40, "Petra", "Kovac", 26,
                new List<TeamInvitationDTO>
                {
                    new TeamInvitationDTO("2", "Peter", "Jackson", 30, "Join our team!"),
                    new TeamInvitationDTO("3", "Ivan", "Novak", 24, "We need you!"),
                });

            // Act
            var result = await controller.CreateTeam(teamForCreate);

            // Assert
            //we check that the response type is Created and obtain the team created
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var teamActual = Assert.IsType<TeamDetailDTO>(createdResult.Value);
            Assert.Equal(expectedTeam, teamActual);
        }
    }
}