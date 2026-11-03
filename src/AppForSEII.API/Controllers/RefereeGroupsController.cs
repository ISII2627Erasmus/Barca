using System.Data;
using System.Net;
using System.Security.Claims;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.RefereeDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RefereeGroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RefereeGroupsController> _logger;

        public RefereeGroupsController(
            ApplicationDbContext context,
            ILogger<RefereeGroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(RefereeGroupDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RefereeGroupDTO>> GetRefereeGroup(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(
                    "id",
                    "The referee group ID must be positive.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var group = await _context.RefereeGroups
                .AsNoTracking()
                .Where(item => item.Id == id)
                .Select(item => new RefereeGroupDTO(
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Rules,
                    item.GameId,
                    item.Game.Name,
                    item.Game.Date,
                    item.Game.Place,
                    item.Game.Sport.Name,
                    item.RefereeAssignments
                        .OrderBy(assignment => assignment.Referee.Name)
                        .Select(assignment => new RefereeAssignedToDTO(
                            assignment.RefereeId,
                            assignment.Referee.Name,
                            assignment.Referee.Surname,
                            assignment.Referee.Rating,
                            assignment.Referee.YearsRefereeing,
                            assignment.Role,
                            assignment.RoleDescription,
                            assignment.AcceptedAssignment))
                        .ToList()))
                .FirstOrDefaultAsync();

            if (group == null)
            {
                _logger.LogWarning(
                    "Referee group with ID {GroupId} was not found.",
                    id);
                return NotFound();
            }

            return Ok(group);
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(RefereeGroupDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RefereeGroupDTO>>
            CreateRefereeGroup(RefereeGroupDTO refereeGroupDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (refereeGroupDTO.RefereeAssignments == null ||
                refereeGroupDTO.RefereeAssignments.Count == 0)
            {
                ModelState.AddModelError(
                    "RefereeAssignments",
                    "Select at least one referee.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var game = await _context.Games
                .Include(item => item.Sport)
                .FirstOrDefaultAsync(item =>
                    item.Id == refereeGroupDTO.GameId &&
                    item.ResponsibleId == userId &&
                    item.Date > DateTime.Now);

            if (game == null)
            {
                _logger.LogWarning(
                    "Eligible game {GameId} was not found for user {UserId}.",
                    refereeGroupDTO.GameId,
                    userId);
                return NotFound();
            }

            if (await _context.RefereeGroups.AnyAsync(
                    item => item.GameId == game.Id))
            {
                return Conflict("This game already has a referee group.");
            }

            if (await _context.RefereeGroups.AnyAsync(
                    item => item.Name == refereeGroupDTO.Name))
            {
                return Conflict(
                    $"A referee group with the name '{refereeGroupDTO.Name}' already exists.");
            }

            var refereeIds = refereeGroupDTO.RefereeAssignments
                .Select(item => item.RefereeId)
                .ToList();

            if (refereeIds.Distinct().Count() != refereeIds.Count)
            {
                ModelState.AddModelError(
                    "RefereeAssignments",
                    "A referee can only be selected once.");
            }

            if (refereeIds.Count < game.Sport.NumberOfReferees)
            {
                ModelState.AddModelError(
                    "RefereeAssignments",
                    $"This sport requires at least {game.Sport.NumberOfReferees} referees.");
            }

            var referees = await _context.Referees
                .Where(referee => refereeIds.Contains(referee.Id))
                .ToListAsync();

            if (referees.Count != refereeIds.Distinct().Count())
            {
                ModelState.AddModelError(
                    "RefereeAssignments",
                    "One or more selected referees do not exist.");
            }

            if (referees.Any(referee => referee.SportId != game.SportId))
            {
                ModelState.AddModelError(
                    "RefereeAssignments",
                    "All selected referees must be instructed in the game's sport.");
            }

            var busyRefereeIds = await _context.RefereeAssignments
                .Where(assignment =>
                    assignment.RefereeGroup.Game.Date > DateTime.Now)
                .Select(assignment => assignment.RefereeId)
                .ToListAsync();

            if (refereeIds.Any(id => busyRefereeIds.Contains(id)))
            {
                ModelState.AddModelError(
                    "RefereeAssignments",
                    "One or more selected referees are already assigned to a future game.");
            }

            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var group = new RefereeGroup(
                game,
                refereeGroupDTO.Name,
                refereeGroupDTO.Description,
                refereeGroupDTO.Rules);

            _context.RefereeGroups.Add(group);

            try
            {
                await _context.SaveChangesAsync();

                var assignmentDTOs = new List<RefereeAssignedToDTO>();

                foreach (var selectedReferee in refereeGroupDTO.RefereeAssignments)
                {
                    var referee = referees.First(
                        item => item.Id == selectedReferee.RefereeId);

                    var assignment = new RefereeAssignedTo(
                        group.Id,
                        referee.Id,
                        selectedReferee.Role,
                        selectedReferee.RoleDescription);

                    _context.RefereeAssignments.Add(assignment);

                    assignmentDTOs.Add(new RefereeAssignedToDTO(
                        referee.Id,
                        referee.Name,
                        referee.Surname,
                        referee.Rating,
                        referee.YearsRefereeing,
                        selectedReferee.Role,
                        selectedReferee.RoleDescription,
                        false));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var createdGroup = new RefereeGroupDTO(
                    group.Id,
                    group.Name,
                    group.Description,
                    group.Rules,
                    game.Id,
                    game.Name,
                    game.Date,
                    game.Place,
                    game.Sport.Name,
                    assignmentDTOs);

                return CreatedAtAction(
                    nameof(GetRefereeGroup),
                    new { id = group.Id },
                    createdGroup);
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(
                    exception,
                    "Could not save referee group for game {GameId}.",
                    game.Id);
                return Conflict(
                    "The referee group could not be saved. Check the group name and game.");
            }
        }
    }
}
