using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
    }

    public ApplicationUser(
        string id,
        string name,
        string surname,
        string userName)
    {
        Id = id;
        Name = name;
        Surname = surname;
        UserName = userName;
        Email = userName;
    }

    public ApplicationUser(
        string id,
        string name,
        string surname,
        string userName,
        DateOnly birthDate,
        Gender gender)
        : this(id, name, surname, userName)
    {
        BirthDate = birthDate;
        Gender = gender;
    }

    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Surname { get; set; }

    [Required]
    public DateOnly BirthDate { get; set; }

    [NotMapped]
    public int Age
    {
        get
        {
            if (BirthDate == default)
            {
                return 0;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            int age = today.Year - BirthDate.Year;

            if (BirthDate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }

    [EnumDataType(typeof(Gender))]
    public Gender Gender { get; set; }

    public ICollection<Team> CaptainOf { get; set; }
        = new List<Team>();

    public ICollection<TeamInvitation> TeamInvitations { get; set; }
        = new List<TeamInvitation>();

    public ICollection<InterestedIn> Interested { get; set; }
        = new List<InterestedIn>();
}
