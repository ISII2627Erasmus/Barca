
using AppForSEII.Web.API;

namespace AppForSEII.Web
{
    public class GameStateContainer
    {
        public GameForCreateDTO Game { get; private set; } = new();

        public List<TeamForGameDTO> SelectedTeams { get; private set; } = new();

        public void AddTeam(TeamForGameDTO team, string message)
        {
            Game.GameInvitations ??= new List<GameInvitationDTO>();

            if (!Game.GameInvitations.Any(i => i.TeamId == team.Id))
            {
                Game.GameInvitations.Add(new GameInvitationDTO
                {
                    TeamId = team.Id,
                    Message = message
                });

                SelectedTeams.Add(team);
            }
        }

        public void RemoveTeam(int teamId)
        {
            var invitation = Game.GameInvitations?
                .FirstOrDefault(i => i.TeamId == teamId);

            if (invitation != null)
            {
                Game.GameInvitations!.Remove(invitation);
            }

            SelectedTeams.RemoveAll(t => t.Id == teamId);
        }

        public void ClearGame()
        {
            Game = new GameForCreateDTO();
            SelectedTeams.Clear();
        }
    }
}
