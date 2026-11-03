using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.RefereeDTOs;

public class RefereeGroupDTO
{
    public RefereeGroupDTO()
    {
    }

    public RefereeGroupDTO(
        int gameId,
        string name,
        string? description,
        string? rules,
        List<RefereeAssignedToDTO> refereeAssignments)
    {
        GameId = gameId;
        Name = name;
        Description = description;
        Rules = rules;
        RefereeAssignments = refereeAssignments;
    }

    public RefereeGroupDTO(
        int id,
        string name,
        string? description,
        string? rules,
        int gameId,
        string gameName,
        DateTime gameDate,
        string gamePlace,
        string sportName,
        List<RefereeAssignedToDTO> refereeAssignments)
    {
        Id = id;
        Name = name;
        Description = description;
        Rules = rules;
        GameId = gameId;
        GameName = gameName;
        GameDate = gameDate;
        GamePlace = gamePlace;
        SportName = sportName;
        RefereeAssignments = refereeAssignments;
    }

    public int Id { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Rules { get; set; }

    [Range(1, int.MaxValue)]
    public int GameId { get; set; }

    public string? GameName { get; set; }
    public DateTime GameDate { get; set; }
    public string? GamePlace { get; set; }
    public string? SportName { get; set; }

    [Required]
    [MinLength(1)]
    public List<RefereeAssignedToDTO> RefereeAssignments { get; set; } = new();

    public override bool Equals(object? obj)
    {
        return obj is RefereeGroupDTO dto &&
               Id == dto.Id &&
               Name == dto.Name &&
               Description == dto.Description &&
               Rules == dto.Rules &&
               GameId == dto.GameId &&
               GameName == dto.GameName &&
               GameDate == dto.GameDate &&
               GamePlace == dto.GamePlace &&
               SportName == dto.SportName &&
               RefereeAssignments.SequenceEqual(dto.RefereeAssignments);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Name);
        hash.Add(Description);
        hash.Add(Rules);
        hash.Add(GameId);
        hash.Add(GameName);
        hash.Add(GameDate);
        hash.Add(GamePlace);
        hash.Add(SportName);

        foreach (var assignment in RefereeAssignments)
        {
            hash.Add(assignment);
        }

        return hash.ToHashCode();
    }
}
