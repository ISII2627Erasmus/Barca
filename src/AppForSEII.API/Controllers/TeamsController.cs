using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.DTOs.ApplicationUserDTO;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.TeamDTOs;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeamsController> _logger;



        public TeamsController(
            ApplicationDbContext context,
            ILogger<TeamsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]

        [Route("[action]")]

        [ProducesResponseType(typeof(decimal), (int)HttpStatusCode.OK)]

        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]

        public async Task<ActionResult> ComputeDivision(decimal op1, decimal op2)

        {

            if (op2 == 0)

            {

                _logger.LogError($"{DateTime.Now} Exception: op2=0, division by 0");

                return BadRequest("op2 must be different from 0");

            }

            decimal result = decimal.Round(op1 / op2, 2);

            return Ok(result);

        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(List<TeamForGameDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TeamForGameDTO>>> GetTeamsForGame(
            string? captainName, string? sportName)
        {
            var query = _context.Teams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(captainName))
            {
                query = query.Where(team => team.Captain.Name == captainName);
            }

            if (!string.IsNullOrWhiteSpace(sportName))
            {
                query = query.Where(team => team.Sport.Name == sportName);

            }

            var teams = await query
            .OrderBy(team => team.Name)
            .ThenBy(team => team.Captain.Surname)
            .Select(team => new TeamForGameDTO

            {

                Id = team.Id,
                Name = team.Name,
                CaptainName = team.Captain.Name ?? "",
                CaptainSurname = team.Captain.Surname ?? "",
                NumberOfMembers = 1 + team.TeamInvitations.Count(invitation => invitation.InvitationAccepted),
                MinAge = team.MinAge,
                MaxAge = team.MaxAge,
                SportName = team.Sport.Name
            })

            .ToListAsync();


            return Ok(teams);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(TeamDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetTeam(int id)
        {
            TeamDetailDTO? team = await _context.Teams
                .Where(t => t.Id == id)
                .Include(t => t.Captain)
                .Include(t => t.Sport)
                .Include(t => t.TeamInvitations)
                    .ThenInclude(ti => ti.User)
                .Select(t => new TeamDetailDTO(t.Id, t.Name, t.Sport.Name, t.MinAge, t.MaxAge,
                    t.Captain.Name, t.Captain.Surname, t.Captain.Age,
                    t.TeamInvitations
                        .Select(ti => new TeamInvitationDTO(ti.User.Id, ti.User.Name, ti.User.Surname,
                            ti.User.Age, ti.InvitationMessage))
                        .ToList()))
                .FirstOrDefaultAsync();

            if (team == null)
            {
                _logger.LogError($"{DateTime.Now} Error: Team with id {id} does not exist");
                return NotFound();
            }

            return Ok(team);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(TeamDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateTeam(TeamForCreateDTO teamForCreate)
        {
            if (teamForCreate.MinAge > teamForCreate.MaxAge)
            {
                ModelState.AddModelError("MinAge&MaxAge", "Error! The minimum age must be lower than or equal to the maximum age");
            }
            if (teamForCreate.TeamInvitations.Count == 0)
            {
                ModelState.AddModelError("TeamInvitations", "Error! The team must have at least one invitation");
            }
            if (teamForCreate.TeamInvitations.Count + 1 > teamForCreate.MaxMembers)
            {
                ModelState.AddModelError("MaxMembers", "Error! The maximum number of members must be greater than or equal to the number of invitations + 1 (captain)");
            }

            var captain = await _context.Users.FirstOrDefaultAsync(u => u.UserName == teamForCreate.CaptainUserName);
            if (captain == null)
            {
                ModelState.AddModelError("CaptainUserName", $"Error! The captain with username {teamForCreate.CaptainUserName} does not exist");
            }

            var sport = await _context.Sports.FirstOrDefaultAsync(s => s.Name == teamForCreate.SportName);
            if (sport == null)
            {
                ModelState.AddModelError("SportName", $"Error! The sport with name {teamForCreate.SportName} does not exist");
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (await _context.Teams.AnyAsync(t => t.Name == teamForCreate.Name))
            {
                _logger.LogError($"{DateTime.Now} Error: Team with name {teamForCreate.Name} already exists");
                return Conflict($"A team with the name '{teamForCreate.Name}' already exists.");
            }

            if (teamForCreate.TeamInvitations.Count + 1 < sport!.MinimumNumberOfPlayers)
            {
                ModelState.AddModelError("TeamInvitations", $"Error! {sport.Name} needs at least {sport.MinimumNumberOfPlayers} players, please select additional teammates");
            }

            var invitedIds = teamForCreate.TeamInvitations.Select(ti => ti.UserId).ToList();
            if (invitedIds.Distinct().Count() != invitedIds.Count)
            {
                ModelState.AddModelError("TeamInvitations", "Error! There are duplicate invitations");
            }

            var invitedUsers = await _context.Users
                .Where(u => invitedIds.Contains(u.Id))
                .ToListAsync();

            Team team = new Team
            {
                Name = teamForCreate.Name,
                MaxMembers = teamForCreate.MaxMembers,
                MinAge = teamForCreate.MinAge,
                MaxAge = teamForCreate.MaxAge,
                Captain = captain!,
                Sport = sport,
                Description = teamForCreate.Description,
            };

            foreach (var invitation in teamForCreate.TeamInvitations)
            {
                var user = invitedUsers.FirstOrDefault(u => u.Id == invitation.UserId);
                if (user == null)
                {
                    ModelState.AddModelError("TeamInvitations", $"Error! The player with id '{invitation.UserId}' does not exist");
                }

                else if (user.Id == captain!.Id)
                {
                    ModelState.AddModelError("TeamInvitations", $"Error! The captain cannot be invited to the team");
                }
                else
                {
                    team.TeamInvitations.Add(new TeamInvitation
                    {
                        UserId = user.Id,
                        User = user,
                        Team = team,
                        InvitationMessage = invitation.InvitationMessage,
                        InvitationAccepted = false
                    });
                }
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _context.Teams.Add(team);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("Database", "Error! Could not save the team to the database");
                return Conflict("Error" + ex.Message);
            }

            var teamDetail = new TeamDetailDTO(team.Id, team.Name, team.Sport.Name, team.MinAge, team.MaxAge,
                captain!.Name, captain.Surname, captain.Age,
                team.TeamInvitations
                    .Select(ti => new TeamInvitationDTO(ti.User.Id, ti.User.Name, ti.User.Surname,
                        ti.User.Age, ti.InvitationMessage))
                    .ToList());

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, teamDetail);
        }
    }


}