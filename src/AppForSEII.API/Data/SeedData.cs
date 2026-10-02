namespace AppForSEII.API.Data
{
    public class SeedData
    {
        public static void Initialize(
            ApplicationDbContext dbContext,
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            List<string> rolesNames = new List<string> {"Administrator","Employee","Customer"};

            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"An error occurred seeding the roles in the Database.");
            }

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            try
            {
                SeedUsers(userManager, rolesNames);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"An error occurred seeding the Users in the Database.");
            }

            try
            {
                SeedSports(dbContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"An error occurred seeding Sports in the Database.");
            }

            try
            {
                SeedItems(dbContext);
            }
                catch (Exception ex)
                {
                    logger.LogError(ex,"An error occurred seeding Items for Purchase Sport Items in the Database.");
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
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    role.NormalizedName = roleName;
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }
        }
        public static void SeedUsers(
            UserManager<ApplicationUser> userManager,
            List<string> roles)
        {
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null)
            {
                ApplicationUser user = new ApplicationUser(
                    "1",
                    "Elena",
                    "Navarro Martínez",
                    "elena@uclm.es"
                );

                user.EmailConfirmed = true;

                var result =
                    userManager.CreateAsync(user, "Password1234%");

                result.Wait();

                if (result.IsCompletedSuccessfully)
                {
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }


            if (userManager.FindByNameAsync("peter@uclm.es").Result == null)
            {

                ApplicationUser user = new ApplicationUser(
                    "3",
                    "Peter",
                    "Jackson",
                    "peter@uclm.es"
                );

                user.EmailConfirmed = true;

                var result =
                    userManager.CreateAsync(user, "OtherPass12$");

                result.Wait();

                if (result.IsCompletedSuccessfully)
                {
                    // Customer role
                    userManager.AddToRoleAsync(user, roles[2]).Wait();
                }
            }
        }


        public static void SeedSports(ApplicationDbContext dbContext)
        {
            if (!dbContext.Sports.Any())
            {
                dbContext.Sports.AddRange(
                    new Sport
                    {
                        Name = "Football",
                        MinimumNumberOfPlayers = 11,
                        NumberOfReferees = 1,
                        Description = "Football sport",
                        BasicRules = "Two teams compete to score goals."
                    },
                    new Sport
                    {
                        Name = "Basketball",
                        MinimumNumberOfPlayers = 5,
                        NumberOfReferees = 2,
                        Description = "Basketball sport",
                        BasicRules = "Two teams compete to score points."
                    }
                );

                dbContext.SaveChanges();
            }
        }
        public static void SeedItems(ApplicationDbContext dbContext)
        {
            var football = dbContext.Sports.SingleOrDefault(s => s.Name == "Football")
                ?? throw new InvalidOperationException("SeedSports must run before SeedItems.");

            var male = dbContext.Genders.SingleOrDefault(g => g.Name == "Male")
                ?? throw new InvalidOperationException("The gender-seeding method must run before SeedItems.");

            var female = dbContext.Genders.SingleOrDefault(g => g.Name == "Female")
                ?? throw new InvalidOperationException("The gender-seeding method must run before SeedItems.");

            if (!dbContext.Items.Any(i => i.Name == "Match Football"))
            {
                dbContext.Items.Add(new Item("Match Football", "Adidas",29.99m, 12,male,"Size 5",20,football));
            }

            dbContext.SaveChanges();
        }
    }
}