
using Microsoft.AspNetCore.Mvc;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.GameDTOs;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GamesController> _logger;

        public GamesController(
            ApplicationDbContext context,
            ILogger<GamesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetGameDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                _logger.LogWarning(
                    "Game with ID {GameId} was not found.", id);

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

                InvitedTeams = game.GameInvitations
                    .Select(invitation => new InvitedTeamDTO
                    {
                        TeamId = invitation.Team.Id,
                        TeamName = invitation.Team.Name,
                        CaptainName = invitation.Team.Captain.Name ?? "",
                        CaptainSurname = invitation.Team.Captain.Surname ?? "",
                        NumberOfMembers = 1 + invitation.Team.TeamInvitations
                            .Count(memberInvitation =>
                                memberInvitation.InvitationAccepted),
                        MinAge = invitation.Team.MinAge,
                        MaxAge = invitation.Team.MaxAge,
                        SportName = invitation.Team.Sport.Name,
                        Message = invitation.Message
                    })
                    .ToList()
            };

            return Ok(gameDTO);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(GetGameDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateGame(
            [FromBody] GameForCreateDTO gameDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(gameDTO.Name))
            {
                return BadRequest("Game name is required.");
            }

            if (gameDTO.Date <= DateTime.Now)
            {
                return BadRequest("Game date must be in the future.");
            }

            if (string.IsNullOrWhiteSpace(gameDTO.Place) ||
                gameDTO.Place.Length < 6)
            {
                return BadRequest(
                    "Game place must contain more than 5 characters.");
            }

            if (gameDTO.GameInvitations == null ||
                gameDTO.GameInvitations.Count == 0)
            {
                return BadRequest("At least one team must be selected.");
            }

            if (gameDTO.GameInvitations.Any(invitation =>
                invitation == null ||
                invitation.TeamId <= 0 ||
                string.IsNullOrWhiteSpace(invitation.Message) ||
                invitation.Message.Length <= 10))
            {
                return BadRequest(
                    "Each invitation must have a valid team and a message longer than 10 characters.");
            }

            var gameNameExists = await _context.Games
                .AnyAsync(game => game.Name == gameDTO.Name);

            if (gameNameExists)
            {
                return Conflict("A game with this name already exists.");
            }

            var teamIds = gameDTO.GameInvitations
                .Select(invitation => invitation.TeamId)
                .ToList();

            if (teamIds.Distinct().Count() != teamIds.Count)
            {
                return BadRequest(
                    "The same team cannot be invited twice.");
            }

            var selectedTeams = await _context.Teams
                .Include(team => team.Sport)
                .Include(team => team.Captain)
                .Include(team => team.TeamInvitations)
                .Where(team => teamIds.Contains(team.Id))
                .ToListAsync();

            if (selectedTeams.Count != teamIds.Count)
            {
                return BadRequest(
                    "One or more selected teams do not exist.");
            }

            var sportId = selectedTeams[0].Sport.Id;

            if (selectedTeams.Any(team => team.Sport.Id != sportId))
            {
                return BadRequest(
                    "All selected teams must belong to the same sport.");
            }

            if (!selectedTeams.Any(team => team.Captain.Id == userId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You must be the captain of at least one selected team.");
            }

            var game = new Game
            {
                Name = gameDTO.Name,
                Date = gameDTO.Date,
                Place = gameDTO.Place,
                Description = gameDTO.Description,
                SportId = sportId,
                ResponsibleId = userId,
                GameInvitations = gameDTO.GameInvitations
                    .Select(invitation => new GameInvitation
                    {
                        TeamId = invitation.TeamId,
                        Message = invitation.Message,
                        AcceptedGame = false
                    })
                    .ToList()
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            var responseDTO = new GetGameDTO
            {
                Id = game.Id,
                Name = game.Name,
                Date = game.Date,
                Place = game.Place,
                Description = game.Description,
                SportName = selectedTeams[0].Sport.Name,
                InvitedTeams = gameDTO.GameInvitations
                    .Select(invitation =>
                    {
                        var team = selectedTeams.First(
                            selectedTeam =>
                                selectedTeam.Id == invitation.TeamId);

                        return new InvitedTeamDTO
                        {
                            TeamId = team.Id,
                            TeamName = team.Name,
                            CaptainName = team.Captain.Name ?? "",
                            CaptainSurname = team.Captain.Surname ?? "",
                            NumberOfMembers = 1 + team.TeamInvitations
                                .Count(memberInvitation =>
                                    memberInvitation.InvitationAccepted),
                            MinAge = team.MinAge,
                            MaxAge = team.MaxAge,
                            SportName = team.Sport.Name,
                            Message = invitation.Message
                        };
                    })
                    .ToList()
            };

            _logger.LogInformation(
                "Game with ID {GameId} was created.",
                game.Id);

            return CreatedAtAction(
                nameof(GetGame),
                new { id = game.Id },
                responseDTO);
        }
    }
}
