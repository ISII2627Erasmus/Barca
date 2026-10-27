namespace AppForSEII.API.DTOs.TeamDTOs
{
    public class TeamForCreateDTO
    {
        public TeamForCreateDTO()
        {
        }
        public TeamForCreateDTO(string name, int maxMembers, int minAge, int maxAge, string sportName, string captainUserName, IList<TeamInvitationDTO> teamInvitations, string? description = null)
        {
            Name = name;
            MaxMembers = maxMembers;
            MinAge = minAge;
            MaxAge = maxAge;
            SportName = sportName;
            CaptainUserName = captainUserName;
            TeamInvitations = teamInvitations;
            Description = description;
        }

        [Required]
        [StringLength(9, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 9 characters long.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Maximum number of players must be at least 1")]

        public int MaxMembers { get; set; }
        [Range(3, int.MaxValue, ErrorMessage = "Minimum age must be at least 3")]
        public int MinAge { get; set; } = 3;

        [Range(3, int.MaxValue, ErrorMessage = "Maximum age must be at least 3")]
        public int MaxAge { get; set; } = 3;

        [Required]
        [StringLength(50)]
        public string SportName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CaptainUserName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public IList<TeamInvitationDTO> TeamInvitations { get; set; } = new List<TeamInvitationDTO>();
        public override bool Equals(object? obj)
        {
            return obj is TeamForCreateDTO dTO &&
                   Name == dTO.Name &&
                   MaxMembers == dTO.MaxMembers &&
                   MinAge == dTO.MinAge &&
                   MaxAge == dTO.MaxAge &&
                   SportName == dTO.SportName &&
                   CaptainUserName == dTO.CaptainUserName &&
                   Description == dTO.Description &&
                   TeamInvitations.SequenceEqual(dTO.TeamInvitations);}

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, MaxMembers, MinAge, MaxAge, SportName, CaptainUserName, Description);
        }
    }
}