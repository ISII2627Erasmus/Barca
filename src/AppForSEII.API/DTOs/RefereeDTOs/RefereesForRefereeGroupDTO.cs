namespace AppForSEII.API.DTOs.RefereeDTOs;

public class RefereesForRefereeGroupDTO
{
    public RefereesForRefereeGroupDTO(
        int gameId,
        string gameName,
        DateTime gameDate,
        string gamePlace,
        string sportName,
        int numberOfReferees,
        List<RefereeForRefereeGroupDTO> referees)
    {
        GameId = gameId;
        GameName = gameName;
        GameDate = gameDate;
        GamePlace = gamePlace;
        SportName = sportName;
        NumberOfReferees = numberOfReferees;
        Referees = referees;
    }

    public int GameId { get; set; }
    public string GameName { get; set; }
    public DateTime GameDate { get; set; }
    public string GamePlace { get; set; }
    public string SportName { get; set; }
    public int NumberOfReferees { get; set; }
    public List<RefereeForRefereeGroupDTO> Referees { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is RefereesForRefereeGroupDTO dto &&
               GameId == dto.GameId &&
               GameName == dto.GameName &&
               GameDate == dto.GameDate &&
               GamePlace == dto.GamePlace &&
               SportName == dto.SportName &&
               NumberOfReferees == dto.NumberOfReferees &&
               Referees.SequenceEqual(dto.Referees);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GameId);
        hash.Add(GameName);
        hash.Add(GameDate);
        hash.Add(GamePlace);
        hash.Add(SportName);
        hash.Add(NumberOfReferees);

        foreach (var referee in Referees)
        {
            hash.Add(referee);
        }

        return hash.ToHashCode();
    }
}
