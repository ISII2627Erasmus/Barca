using Microsoft.AspNetCore.Mvc;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.GameDTOs;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var game = await _context.Games
                .Include(g => g.Sport)
                .Include(g => g.GameInvitations)
                    .ThenInclude(invitation => invitation.Team)
                        .ThenInclude(team => team.Captain)
                .Include(g => g.GameInvitations)
                    .ThenInclude(invitation => invitation.Team)
                        .ThenInclude(team => team.Sport)
                .Include(g => g.GameInvitations)
                    .ThenInclude(invitation => invitation.Team)
                        .ThenInclude(team => team.TeamInvitations)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            var gameDTO = new GetGameDTO
            {
                Id = game.Id,
                Name = game.Name,
                Date = game.Date,
                Place = game.Place,
                Description = game.Description,
                SportName = game.Sport.Name,

                InvitedTeams = game.GameInvitations.Select(invitation => new InvitedTeamDTO
                {
                    TeamId = invitation.Team.Id,
                    TeamName = invitation.Team.Name,
                    CaptainName = invitation.Team.Captain.Name ?? "",
                    CaptainSurname = invitation.Team.Captain.Surname ?? "",
                    NumberOfMembers = 1 + invitation.Team.TeamInvitations.Count(
                        memberInvitation => memberInvitation.InvitationAccepted),
                    MinAge = invitation.Team.MinAge,
                    MaxAge = invitation.Team.MaxAge,
                    SportName = invitation.Team.Sport.Name,
                    Message = invitation.Message
                }).ToList()
            };

            return Ok(gameDTO);
        }
    }
}