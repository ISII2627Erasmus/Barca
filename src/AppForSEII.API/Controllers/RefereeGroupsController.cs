using System.Net;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.RefereeDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [ProducesResponseType(
            typeof(RefereeGroupDetailDTO),
            (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RefereeGroupDetailDTO>>
            GetRefereeGroup(int id)
        {
            if (!ModelState.IsValid || id <= 0)
            {
                return BadRequest("The referee group ID must be positive.");
            }

            var group = await _context.RefereeGroups
                .AsNoTracking()
                .Include(item => item.Game)
                    .ThenInclude(game => game.Sport)
                .Include(item => item.RefereeAssignments)
                    .ThenInclude(assignment => assignment.Referee)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (group == null)
            {
                _logger.LogWarning(
                    "Referee group with ID {GroupId} was not found.",
                    id);

                return NotFound();
            }

            var groupDTO = new RefereeGroupDetailDTO
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Rules = group.Rules,
                GameId = group.GameId,
                GameName = group.Game.Name,
                GameDate = group.Game.Date,
                GamePlace = group.Game.Place,
                SportName = group.Game.Sport.Name,
                RefereeAssignments = group.RefereeAssignments
                    .Select(assignment => new RefereeAssignmentDetailDTO
                    {
                        RefereeId = assignment.RefereeId,
                        RefereeName = assignment.Referee.Name,
                        RefereeSurname = assignment.Referee.Surname,
                        Rating = assignment.Referee.Rating,
                        YearsRefereeing =
                            assignment.Referee.YearsRefereeing,
                        Role = assignment.Role,
                        RoleDescription = assignment.RoleDescription,
                        AcceptedAssignment =
                            assignment.AcceptedAssignment
                    })
                    .ToList()
            };

            return Ok(groupDTO);
        }
    }
}