using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.RefereeDTOs;

public class RefereeAssignedToDTO
{
    public RefereeAssignedToDTO()
    {
    }

    public RefereeAssignedToDTO(
        string refereeId,
        string role,
        string roleDescription)
    {
        RefereeId = refereeId;
        Role = role;
        RoleDescription = roleDescription;
    }

    public RefereeAssignedToDTO(
        string refereeId,
        string? refereeName,
        string? refereeSurname,
        int rating,
        int yearsRefereeing,
        string role,
        string roleDescription,
        bool acceptedAssignment)
    {
        RefereeId = refereeId;
        RefereeName = refereeName;
        RefereeSurname = refereeSurname;
        Rating = rating;
        YearsRefereeing = yearsRefereeing;
        Role = role;
        RoleDescription = roleDescription;
        AcceptedAssignment = acceptedAssignment;
    }

    [Required]
    public string RefereeId { get; set; } = string.Empty;

    public string? RefereeName { get; set; }
    public string? RefereeSurname { get; set; }
    public int Rating { get; set; }
    public int YearsRefereeing { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string RoleDescription { get; set; } = string.Empty;

    public bool AcceptedAssignment { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is RefereeAssignedToDTO dto &&
               RefereeId == dto.RefereeId &&
               RefereeName == dto.RefereeName &&
               RefereeSurname == dto.RefereeSurname &&
               Rating == dto.Rating &&
               YearsRefereeing == dto.YearsRefereeing &&
               Role == dto.Role &&
               RoleDescription == dto.RoleDescription &&
               AcceptedAssignment == dto.AcceptedAssignment;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            RefereeId,
            RefereeName,
            RefereeSurname,
            Rating,
            YearsRefereeing,
            Role,
            RoleDescription,
            AcceptedAssignment);
    }
}
