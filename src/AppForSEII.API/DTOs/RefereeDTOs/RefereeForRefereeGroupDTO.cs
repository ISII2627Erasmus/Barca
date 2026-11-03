namespace AppForSEII.API.DTOs.RefereeDTOs;

public class RefereeForRefereeGroupDTO
{
    public RefereeForRefereeGroupDTO(
        string id,
        string? name,
        string? surname,
        int rating,
        int yearsRefereeing)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Rating = rating;
        YearsRefereeing = yearsRefereeing;
    }

    public string Id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int Rating { get; set; }
    public int YearsRefereeing { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is RefereeForRefereeGroupDTO dto &&
               Id == dto.Id &&
               Name == dto.Name &&
               Surname == dto.Surname &&
               Rating == dto.Rating &&
               YearsRefereeing == dto.YearsRefereeing;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Surname, Rating, YearsRefereeing);
    }
}
