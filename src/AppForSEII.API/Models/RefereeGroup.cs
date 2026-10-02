using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class RefereeGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Rules { get; set; }

        public int GameId { get; set; }
    }
}