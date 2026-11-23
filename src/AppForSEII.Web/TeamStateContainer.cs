using AppForSEII.Web.API;

namespace AppForSEII.Web
{
    public class TeamStateContainer
    {
        public TeamForCreateDTO Team { get; private set; } = new TeamForCreateDTO()
        {
            TeamInvitations = new List<TeamInvitationDTO>()
        };

        public bool IsSelected(ApplicationUserForTeamDTO player) =>
            Team.TeamInvitations.Any(ti => ti.UserId == player.Id);

        public List<ApplicationUserForTeamDTO> SelectedPlayers { get; private set; } = new 
        List<ApplicationUserForTeamDTO>();


        public void AddPlayerToTeam(ApplicationUserForTeamDTO player)
        {
            if (!IsSelected(player))
            {
                Team.TeamInvitations.Add(new TeamInvitationDTO()
                {
                    UserId = player.Id,
                    Name = player.Name,
                    Surname = player.Surname,
                    Age = player.Age,
                    InvitationMessage = ""
                });
                SelectedPlayers.Add(player);
            }
        }

        public void RemovePlayerFromTeam(TeamInvitationDTO invitation)
        {
            Team.TeamInvitations.Remove(invitation);
            SelectedPlayers.RemoveAll(p => p.Id == invitation.UserId); 
        }

        public void ClearTeam()
        {
            Team = new TeamForCreateDTO() { TeamInvitations = new List<TeamInvitationDTO>() };
            SelectedPlayers = new List<ApplicationUserForTeamDTO>();
        }
    }
}