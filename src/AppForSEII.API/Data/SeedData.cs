namespace AppForSEII.API.Data
{
    public class SeedData
    {
        public static void Initialize(
            ApplicationDbContext dbContext,
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            List<string> rolesNames = new List<string>
            {
                "Administrator",
                "Employee",
                "Customer"
            };

            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the roles in the Database.");
            }

            Gender male;
            try
            {
                male = SeedGender(dbContext, "Male");
                SeedGender(dbContext, "Female");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding Genders in the Database.");
                return;
            }

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            try
            {
                SeedUsers(userManager, rolesNames, male);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the Users in the Database.");
            }

            try
            {
                SeedSports(dbContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding Sports in the Database.");
                return;
            }

            try
            {
                SeedItems(dbContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding Items for Purchase Sport Items in the Database.");
            }

            try
            {
                SeedRefereesAndGame(dbContext, userManager, male);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding referees and games in the Database.");
            }
        }

        public static void SeedRoles(
            RoleManager<IdentityRole> roleManager,
            List<string> roles)
        {
            foreach (string roleName in roles)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    IdentityRole role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName
                    };

                    roleManager.CreateAsync(role).GetAwaiter().GetResult();
                }
            }
        }

        public static void SeedUsers(
            UserManager<ApplicationUser> userManager,
            List<string> roles,
            Gender gender)
        {
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null)
            {
                ApplicationUser user = new ApplicationUser(
                    "1",
                    "Elena",
                    "Navarro Martínez",
                    "elena@uclm.es",
                    new DateOnly(1990, 1, 1),
                    gender)
                {
                    EmailConfirmed = true
                };

                var result = userManager
                    .CreateAsync(user, "Password1234%")
                    .GetAwaiter()
                    .GetResult();

                if (result.Succeeded)
                {
                    userManager
                        .AddToRoleAsync(user, roles[0])
                        .GetAwaiter()
                        .GetResult();
                }
            }

            if (userManager.FindByNameAsync("peter@uclm.es").Result == null)
            {
                ApplicationUser user = new ApplicationUser(
                    "3",
                    "Peter",
                    "Jackson",
                    "peter@uclm.es",
                    new DateOnly(1988, 1, 1),
                    gender)
                {
                    EmailConfirmed = true
                };

                var result = userManager
                    .CreateAsync(user, "OtherPass12$")
                    .GetAwaiter()
                    .GetResult();

                if (result.Succeeded)
                {
                    userManager
                        .AddToRoleAsync(user, roles[2])
                        .GetAwaiter()
                        .GetResult();
                }
            }
        }

        public static void SeedSports(ApplicationDbContext dbContext)
        {
            if (!dbContext.Sports.Any(s => s.Name == "Football"))
            {
                dbContext.Sports.Add(new Sport
                {
                    Name = "Football",
                    MinimumNumberOfPlayers = 11,
                    NumberOfReferees = 1,
                    Description = "Football sport",
                    BasicRules = "Two teams compete to score goals."
                });
            }

            if (!dbContext.Sports.Any(s => s.Name == "Basketball"))
            {
                dbContext.Sports.Add(new Sport
                {
                    Name = "Basketball",
                    MinimumNumberOfPlayers = 5,
                    NumberOfReferees = 2,
                    Description = "Basketball sport",
                    BasicRules = "Two teams compete to score points."
                });
            }

            dbContext.SaveChanges();
        }

        public static void SeedItems(ApplicationDbContext dbContext)
        {
            var football = dbContext.Sports.SingleOrDefault(s => s.Name == "Football")
                ?? throw new InvalidOperationException("SeedSports must run before SeedItems.");

            var male = dbContext.Genders.SingleOrDefault(g => g.Name == "Male")
                ?? throw new InvalidOperationException("Male gender must be seeded before SeedItems.");

            if (!dbContext.Items.Any(i => i.Name == "Match Football"))
            {
                dbContext.Items.Add(
                    new Item(
                        "Match Football",
                        "Adidas",
                        29.99m,
                        12,
                        male,
                        "Size 5",
                        20,
                        football));
            }

            dbContext.SaveChanges();
        }

        public static Gender SeedGender(ApplicationDbContext dbContext, string name)
        {
            var gender = dbContext.Genders.SingleOrDefault(g => g.Name == name);

            if (gender == null)
            {
                gender = new Gender { Name = name };
                dbContext.Genders.Add(gender);
                dbContext.SaveChanges();
            }

            return gender;
        }

        public static void SeedRefereesAndGame(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            Gender gender)
        {
            var football = dbContext.Sports.Single(s => s.Name == "Football");

            SeedRefereeIfMissing(
                userManager,
                "referee1@uclm.es",
                "4",
                "Alex",
                "Rivera",
                football.Id,
                4,
                5,
                gender);

            SeedRefereeIfMissing(
                userManager,
                "referee2@uclm.es",
                "5",
                "Sam",
                "Lopez",
                football.Id,
                3,
                2,
                gender);

            var responsible = userManager
                .FindByNameAsync("elena@uclm.es")
                .GetAwaiter()
                .GetResult()
                ?? throw new InvalidOperationException(
                    "SeedUsers must run before seeding the game.");

            if (!dbContext.Games.Any(g => g.Name == "Football Friendly"))
            {
                dbContext.Games.Add(new Game
                {
                    Name = "Football Friendly",
                    Date = DateTime.Today.AddDays(7),
                    Place = "Main Stadium",
                    Description = "Seed game for adding a referee group.",
                    SportId = football.Id,
                    ResponsibleId = responsible.Id
                });

                dbContext.SaveChanges();
            }
        }

        private static void SeedRefereeIfMissing(
            UserManager<ApplicationUser> userManager,
            string email,
            string id,
            string name,
            string surname,
            int sportId,
            int rating,
            int yearsRefereeing,
            Gender gender)
        {
            if (userManager.FindByNameAsync(email).GetAwaiter().GetResult() != null)
            {
                return;
            }

            var referee = new Referee(
                id,
                name,
                surname,
                email,
                sportId,
                rating,
                yearsRefereeing)
            {
                BirthDate = new DateOnly(1985, 1, 1),
                Gender = gender,
                EmailConfirmed = true
            };

            var result = userManager
                .CreateAsync(referee, "RefereePass123!")
                .GetAwaiter()
                .GetResult();

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not seed referee {email}: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}