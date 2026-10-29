using System.Net;
using System.Security.Claims;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.RefereeDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RefereesForRefereeGroupDTO>>
            GetRefereesForRefereeGroup(
                int? gameId = null,
                int? minYearsRefereeing = null)
        {
            if (minYearsRefereeing.HasValue && minYearsRefereeing.Value < 1)
            {
                ModelState.AddModelError(
                    "minYearsRefereeing",
                    "Years refereeing must be at least 1.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var games = _context.Games
                .Where(game =>
                    game.ResponsibleId == userId &&
                    game.Date > DateTime.Now &&
                    game.RefereeGroup == null);

            if (gameId.HasValue)
            {
                games = games.Where(game => game.Id == gameId.Value);
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
                    "No eligible game was found for user {UserId}.",
                    userId);
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
