namespace AppForSEII.API.Models;

public class Referee : ApplicationUser
{
    public int Rating { get; set; }

    public int YearsRefereeing { get; set; }
}
