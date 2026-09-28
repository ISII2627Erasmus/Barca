using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models;

public class Referee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Years refereeing must be higher than 0.")]
    public int YearsRefereeing { get; set; }

    [Required]
    [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0 and 5.")]
    public double Rating { get; set; }
}