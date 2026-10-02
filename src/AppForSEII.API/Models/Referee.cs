using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models;

public class Referee : ApplicationUser
{
    public Referee()
    {
    }

    public Referee(
        string id,
        string name,
        string surname,
        string userName,
        int sportId,
        int rating,
        int yearsRefereeing)
        : base(id, name, surname, userName)
    {
        SportId = sportId;
        Rating = rating;
        YearsRefereeing = yearsRefereeing;
    }

    [Required]
    public int SportId { get; set; }

    [ForeignKey(nameof(SportId))]
    public Sport Sport { get; set; } = null!;

    [Range(0, 5)]
    public int Rating { get; set; }

    [Range(1, int.MaxValue)]
    public int YearsRefereeing { get; set; }

    public ICollection<RefereeAssignedTo> RefereeAssignments { get; set; }
        = new List<RefereeAssignedTo>();
}