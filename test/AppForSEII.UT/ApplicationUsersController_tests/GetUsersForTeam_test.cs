using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ApplicationUserDTO;

namespace AppForSEII.UT.ApplicationUsersController_tests
{
    public class GetUsersForTeam_test : AppForSEII4SqliteUT
    {
        public GetUsersForTeam_test()
        {
            var sports = new List<Sport>()
            {
                new Sport { Name = "Basketball", MinimumNumberOfPlayers = 5, NumberOfReferees = 2,
                            Description = "Basketball description", BasicRules = "Basketball rules" },
                new Sport { Name = "Futsal", MinimumNumberOfPlayers = 5, NumberOfReferees = 2,
                            Description = "Futsal description", BasicRules = "Futsal rules" },
            };

            var users = new List<ApplicationUser>()
            {
                new ApplicationUser("1", "Ana", "Horvat", "ana@uclm.es", new DateOnly(2002, 1, 1), 24, Gender.Female),
                new ApplicationUser("2", "Peter", "Jackson", "peter@uclm.es", new DateOnly(1996, 1, 1), 30, Gender.Male),
                new ApplicationUser("3", "Luka", "Novak", "luka@uclm.es", new DateOnly(1998, 1, 1), 28, Gender.Male),
                new ApplicationUser("4", "Sam", "Lopez", "sam@uclm.es", new DateOnly(2004, 1, 1), 22, Gender.Male),
            };

            _context.AddRange(sports);
            _context.AddRange(users);
            _context.SaveChanges();

            var interests = new List<InterestedIn>()
            {
                new InterestedIn("1", sports[0].Id, 5),
            new InterestedIn("1", sports[1].Id, 3),
new InterestedIn("2", sports[1].Id, 4),
new InterestedIn("3", sports[1].Id, 2),
new InterestedIn("4", sports[0].Id, 3),
            };

            _context.AddRange(interests);
            _context.SaveChanges();
        }

        public static IEnumerable<object?[]> TestCasesFor_GetUsersForTeam_OK()
        {

            var ana = new ApplicationUserForTeamDTO("1", "Ana", "Horvat", 24,
                new List<InterestedInDTO> { new InterestedInDTO("Basketball", 5), new InterestedInDTO("Futsal", 3) });

            var peter = new ApplicationUserForTeamDTO("2", "Peter", "Jackson", 30,
                new List<InterestedInDTO> { new InterestedInDTO("Futsal", 4) });

            var luka = new ApplicationUserForTeamDTO("3", "Luka", "Novak", 28,
                new List<InterestedInDTO> { new InterestedInDTO("Futsal", 2) });

            var sam = new ApplicationUserForTeamDTO("4", "Sam", "Lopez", 22,
                new List<InterestedInDTO> { new InterestedInDTO("Basketball", 3) });

            //the method returns the users ordered by surname
            var TC1WithoutFilters = new List<ApplicationUserForTeamDTO> { ana, peter, sam, luka };
            var TC2SportFilter = new List<ApplicationUserForTeamDTO> { ana, peter, luka };
            var TC3SkillFilter = new List<ApplicationUserForTeamDTO> { ana, peter };
            var TC4SportAndSkillFilter = new List<ApplicationUserForTeamDTO> { ana };
            var TC5AgeFilter = new List<ApplicationUserForTeamDTO> { peter, luka };

            var allTests = new List<object?[]>
            {   //sportName, skill, minAge, maxAge, expected users
                new object?[] { null, null, null, null, TC1WithoutFilters},
                new object?[] { "Futsal", null, null, null, TC2SportFilter},
                new object?[] { null, 4, null, null, TC3SkillFilter},
                new object?[] { "Basketball", 5, null, null, TC4SportAndSkillFilter},
                new object?[] { null, null, 25, 30, TC5AgeFilter},
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetUsersForTeam_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetUsersForTeam_OK_test(string? sportName, int? skill, int? minAge, int? maxAge,
            IList<ApplicationUserForTeamDTO> expectedUsers)
        {
            // Arrange
            var mock = new Mock<ILogger<ApplicationUsersController>>();
            ILogger<ApplicationUsersController> logger = mock.Object;
            var controller = new ApplicationUsersController(_context, logger);

            // Act
            var result = await controller.GetUsersForTeam(sportName, skill, minAge, maxAge);

            //Assert
            //we check that the response type is OK 
            var okResult = Assert.IsType<OkObjectResult>(result);
            var usersActual = Assert.IsType<List<ApplicationUserForTeamDTO>>(okResult.Value);
            Assert.Equal(expectedUsers, usersActual);

        }

        public static IEnumerable<object?[]> TestCasesFor_GetUsersForTeam_BadRequest()
        {
            var allTests = new List<object?[]>
            {   //skill, minAge, maxAge, expected error
                new object?[] { null, 30, 20, "minAge must be lower than or equal to maxAge"},
                new object?[] { 0, null, null, "skill must be between 1 and 5"},
                new object?[] { 6, null, null, "skill must be between 1 and 5"},
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetUsersForTeam_BadRequest))]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetUsersForTeam_BadRequest_test(int? skill, int? minAge, int? maxAge, string expectedError)
        {
            // Arrange
            var mock = new Mock<ILogger<ApplicationUsersController>>();
            ILogger<ApplicationUsersController> logger = mock.Object;
            var controller = new ApplicationUsersController(_context, logger);

            // Act
            var result = await controller.GetUsersForTeam(null, skill, minAge, maxAge);

            //Assert
            //we check that the response type is BadRequest and obtain the error returned
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            Assert.Equal(expectedError, problemDetails.Errors.First().Value[0]);
        }

    }
}
