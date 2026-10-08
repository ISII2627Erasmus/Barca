
using System;
using System.Collections.Generic;
using System.Linq;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Data;

public class SeedData
{
    public static void Initialize(
        ApplicationDbContext dbContext,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = new List<string>
        {
            "Administrator",
            "Employee",
            "Customer"
        };

        SeedRoles(roleManager, roles);
        SeedUsers(userManager, roles);
        SeedSports(dbContext);
        SeedInterests(dbContext, userManager);

        logger.LogInformation("Seed data initialized successfully.");
    }

    private static void SeedRoles(
        RoleManager<IdentityRole> roleManager,
        List<string> roles)
    {
        foreach (string roleName in roles)
        {
            bool roleExists = roleManager
                .RoleExistsAsync(roleName)
                .GetAwaiter()
                .GetResult();

            if (!roleExists)
            {
                var result = roleManager
                    .CreateAsync(new IdentityRole(roleName))
                    .GetAwaiter()
                    .GetResult();

                EnsureSucceeded(result, $"Create role {roleName}");
            }
        }
    }

    private static void SeedUsers(
        UserManager<ApplicationUser> userManager,
        List<string> roles)
    {
        var users = new[]
        {
            (
                Id: "1",
                Name: "Petra",
                Surname: "Petra Peric",
                Email: "petra@alu.uclm.es",
                BirthDate: new DateOnly(1990, 5, 15),
                Age: 36,
                Gender: Gender.Female,
                Role: roles[0],
                Password: "Password1234%"
            ),
            (
                Id: "3",
                Name: "Peter",
                Surname: "Jackson",
                Email: "peter@alu.uclm.es",
                BirthDate: new DateOnly(2001, 3, 10),
                Age: 25,
                Gender: Gender.Male,
                Role: roles[2],
                Password: "OtherPass12$"
            ),
            (
                Id: "seed-ana",
                Name: "Ana",
                Surname: "Horvat",
                Email: "ana@alu.uclm.es",
                BirthDate: new DateOnly(2002, 6, 20),
                Age: 24,
                Gender: Gender.Female,
                Role: roles[2],
                Password: "OtherPass12$"
            ),
            (
                Id: "seed-luka",
                Name: "Luka",
                Surname: "Novak",
                Email: "luka@alu.uclm.es",
                BirthDate: new DateOnly(2000, 9, 5),
                Age: 26,
                Gender: Gender.Male,
                Role: roles[2],
                Password: "OtherPass12$"
            ),
            (
                Id: "seed-mia",
                Name: "Mia",
                Surname: "Kovač",
                Email: "mia@alu.uclm.es",
                BirthDate: new DateOnly(2003, 11, 12),
                Age: 22,
                Gender: Gender.Female,
                Role: roles[2],
                Password: "OtherPass12$"
            ),
            (
                Id: "seed-ivan",
                Name: "Ivan",
                Surname: "Marić",
                Email: "ivan@alu.uclm.es",
                BirthDate: new DateOnly(1999, 1, 25),
                Age: 27,
                Gender: Gender.Male,
                Role: roles[2],
                Password: "OtherPass12$"
            )
        };

        foreach (var data in users)
        {
            ApplicationUser? user = userManager
                .FindByNameAsync(data.Email)
                .GetAwaiter()
                .GetResult();

            if (user == null)
            {
                user = new ApplicationUser(
                    data.Id,
                    data.Name,
                    data.Surname,
                    data.Email)
                {
                    BirthDate = data.BirthDate,
                    Age = data.Age,
                    Gender = data.Gender,
                    EmailConfirmed = true
                };

                IdentityResult createResult = userManager
                    .CreateAsync(user, data.Password)
                    .GetAwaiter()
                    .GetResult();

                EnsureSucceeded(createResult, $"Create user {data.Email}");
            }
            else
            {
                user.Name = data.Name;
                user.Surname = data.Surname;
                user.BirthDate = data.BirthDate;
                user.Age = data.Age;
                user.Gender = data.Gender;

                IdentityResult updateResult = userManager
                    .UpdateAsync(user)
                    .GetAwaiter()
                    .GetResult();

                EnsureSucceeded(updateResult, $"Update user {data.Email}");
            }

            bool alreadyHasRole = userManager
                .IsInRoleAsync(user, data.Role)
                .GetAwaiter()
                .GetResult();

            if (!alreadyHasRole)
            {
                IdentityResult roleResult = userManager
                    .AddToRoleAsync(user, data.Role)
                    .GetAwaiter()
                    .GetResult();

                EnsureSucceeded(roleResult, $"Assign role to {data.Email}");
            }
        }
    }

    public static void SeedSports(ApplicationDbContext dbContext)
    {
        var sportsToSeed = new[]
        {
            new Sport
            {
                Name = "Football",
                MinimumNumberOfPlayers = 11,
                NumberOfReferees = 1,
                Description = "Football is a team sport played between two teams.",
                BasicRules = "Two teams compete to score goals."
            },
            new Sport
            {
                Name = "Basketball",
                MinimumNumberOfPlayers = 5,
                NumberOfReferees = 2,
                Description = "Basketball is a team sport played on a court.",
                BasicRules = "Two teams compete to score points by shooting the ball into the basket."
            },
            new Sport
            {
                Name = "Futsal",
                MinimumNumberOfPlayers = 5,
                NumberOfReferees = 2,
                Description = "Futsal is an indoor version of football.",
                BasicRules = "Two teams of five players compete to score goals."
            }
        };

        foreach (var sport in sportsToSeed)
        {
            var existingSport = dbContext.Sports
                .SingleOrDefault(s => s.Name == sport.Name);

            if (existingSport == null)
            {
                dbContext.Sports.Add(sport);
            }
            else
            {
                existingSport.MinimumNumberOfPlayers =
                    sport.MinimumNumberOfPlayers;

                existingSport.NumberOfReferees =
                    sport.NumberOfReferees;

                existingSport.Description =
                    sport.Description;

                existingSport.BasicRules =
                    sport.BasicRules;
            }
        }

        dbContext.SaveChanges();
    }

    private static void SeedInterests(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var interests = new[]
        {
            (Email: "petra@alu.uclm.es", SportName: "Futsal", Skill: 4),
            (Email: "petra@alu.uclm.es", SportName: "Basketball", Skill: 3),

            (Email: "peter@alu.uclm.es", SportName: "Futsal", Skill: 5),
            (Email: "peter@alu.uclm.es", SportName: "Basketball", Skill: 4),

            (Email: "ana@alu.uclm.es", SportName: "Futsal", Skill: 3),
            (Email: "ana@alu.uclm.es", SportName: "Basketball", Skill: 5),

            (Email: "luka@alu.uclm.es", SportName: "Futsal", Skill: 4),
            (Email: "luka@alu.uclm.es", SportName: "Basketball", Skill: 2),

            (Email: "mia@alu.uclm.es", SportName: "Futsal", Skill: 2),
            (Email: "mia@alu.uclm.es", SportName: "Basketball", Skill: 4),

            (Email: "ivan@alu.uclm.es", SportName: "Futsal", Skill: 5),
            (Email: "ivan@alu.uclm.es", SportName: "Basketball", Skill: 3)
        };

        foreach (var data in interests)
        {
            ApplicationUser user = userManager
                .FindByNameAsync(data.Email)
                .GetAwaiter()
                .GetResult()
                ?? throw new InvalidOperationException(
                    $"Seed user {data.Email} was not found.");

            Sport sport = dbContext.Sports.Single(
                item => item.Name == data.SportName);

            InterestedIn? interest = dbContext.InterestedIns
                .SingleOrDefault(item =>
                    item.UserId == user.Id &&
                    item.SportId == sport.Id);

            if (interest == null)
            {
                dbContext.InterestedIns.Add(new InterestedIn(
                    user.Id,
                    sport.Id,
                    data.Skill));
            }
            else
            {
                interest.Skill = data.Skill;
            }

            dbContext.SaveChanges();
        }

        dbContext.SaveChanges();
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (!result.Succeeded)
        {
            string errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"{operation} failed: {errors}");
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
