using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PlantUmlClassDiagramGenerator.Attributes;

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
        int age,
        Gender gender)
        : this(id, name, surname, userName)
    {
        BirthDate = birthDate;
        Age = age;
        Gender = gender;
    }

    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Surname { get; set; }

    [Required]
    [PlantUmlIgnoreAssociation]
    public DateOnly BirthDate { get; set; }

    public int Age { get; set; }

    [EnumDataType(typeof(Gender))]
    [PlantUmlIgnoreAssociation]
    public Gender Gender { get; set; }

    [PlantUmlAssociation(Name = "Team", Association = "o--",
    LeafLabel = "0..*", Label = "CaptainOf")]
    public ICollection<Team> CaptainOf { get; set; } = new List<Team>();

    [PlantUmlAssociation(Name = "TeamInvitation", Association = "o--",
    LeafLabel = "0..*", Label = "TeamInvitations")]
    public ICollection<TeamInvitation> TeamInvitations { get; set; }
    = new List<TeamInvitation>();

    [PlantUmlAssociation(Name = "InterestedIn", Association = "o--",
    LeafLabel = "0..*", Label = "Interested")]
    public ICollection<InterestedIn> Interested { get; set; }
    = new List<InterestedIn>();
}
