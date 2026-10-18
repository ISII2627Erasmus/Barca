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
            typeof(List<RefereeForRefereeGroupDTO>),
            (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<List<RefereeForRefereeGroupDTO>>>
            GetRefereesForRefereeGroup(
                int gameId,
                int? minRating = null,
                int? minYearsRefereeing = null)
        {
            if (minRating.HasValue &&
                (minRating.Value < 0 || minRating.Value > 5))
            {
                return BadRequest("Minimum rating must be between 0 and 5.");
            }

            if (minYearsRefereeing.HasValue &&
                minYearsRefereeing.Value < 1)
            {
                return BadRequest(
                    "Minimum years refereeing must be at least 1.");
            }

            var game = await _context.Games
                .Where(item => item.Id == gameId)
                .Select(item => new
                {
                    item.Id,
                    item.SportId
                })
                .FirstOrDefaultAsync();

            if (game == null)
            {
                _logger.LogWarning(
                    "Game with ID {GameId} was not found.",
                    gameId);

                return NotFound();
            }

            var query = _context.Referees
                .AsNoTracking()
                .Where(referee => referee.SportId == game.SportId);

            if (minRating.HasValue)
            {
                query = query.Where(
                    referee => referee.Rating >= minRating.Value);
            }

            if (minYearsRefereeing.HasValue)
            {
                query = query.Where(
                    referee =>
                        referee.YearsRefereeing >= minYearsRefereeing.Value);
            }

            var referees = await query
                .OrderBy(referee => referee.Name)
                .ThenBy(referee => referee.Surname)
                .Select(referee => new RefereeForRefereeGroupDTO
                {
                    Id = referee.Id,
                    Name = referee.Name,
                    Surname = referee.Surname,
                    Rating = referee.Rating,
                    YearsRefereeing = referee.YearsRefereeing
                })
                .ToListAsync();

            return Ok(referees);
        }
    }
}