using System.Net;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.RefereeDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefereesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RefereesController> _logger;

        public RefereesController(
            ApplicationDbContext context,
            ILogger<RefereesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(
            typeof(RefereesForRefereeGroupDTO),
            (int)HttpStatusCode.OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RefereesForRefereeGroupDTO>>
            GetRefereesForRefereeGroup(
                string? sportName = null,
                int? minYearsRefereeing = null)
        {
            if (minYearsRefereeing.HasValue && minYearsRefereeing.Value < 1)
            {
                ModelState.AddModelError(
                    "minYearsRefereeing",
                    "Years refereeing must be at least 1.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var games = _context.Games
                .Where(game =>
                    game.Date > DateTime.Now &&
                    game.RefereeGroup == null);

            if (!string.IsNullOrWhiteSpace(sportName))
            {
                games = games.Where(game => game.Sport.Name == sportName);
            }

            var game = await games
                .OrderByDescending(item => item.Id)
                .Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Date,
                    item.Place,
                    item.SportId,
                    SportName = item.Sport.Name,
                    item.Sport.NumberOfReferees
                })
                .FirstOrDefaultAsync();

            if (game == null)
            {
                _logger.LogWarning(
                    "No eligible game was found.");
                return NotFound();
            }

            var refereeQuery = _context.Referees
                .AsNoTracking()
                .Where(referee =>
                    referee.SportId == game.SportId &&
                    referee.YearsRefereeing > 0 &&
                    !referee.RefereeAssignments.Any(assignment =>
                        assignment.RefereeGroup.Game.Date > DateTime.Now));

            if (minYearsRefereeing.HasValue)
            {
                refereeQuery = refereeQuery.Where(referee =>
                    referee.YearsRefereeing >= minYearsRefereeing.Value);
            }

            var referees = await refereeQuery
                .OrderBy(referee => referee.Name)
                .ThenBy(referee => referee.Surname)
                .Select(referee => new RefereeForRefereeGroupDTO(
                    referee.Id,
                    referee.Name,
                    referee.Surname,
                    referee.Rating,
                    referee.YearsRefereeing))
                .ToListAsync();

            if (referees.Count == 0)
            {
                _logger.LogWarning(
                    "No referees are available for game {GameId} and the selected filters.",
                    game.Id);
                return NotFound(
                    "No referees are available for the selected game and filters.");
            }

            var result = new RefereesForRefereeGroupDTO(
                game.Id,
                game.Name,
                game.Date,
                game.Place,
                game.SportName,
                game.NumberOfReferees,
                referees);

            return Ok(result);
        }
    }
}
