using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.DTOs.ApplicationUserDTO;
using AppForSEII.API.Data;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationUsersController> _logger;

        public ApplicationUsersController(ApplicationDbContext context, ILogger<ApplicationUsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ApplicationUsers
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<ApplicationUserDTO>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.ApplicationUsers
            .Select(u => new ApplicationUserDTO(u.Id, u.Name, u.Surname, u.UserName, u.PhoneNumber))
            .ToListAsync());
        }

        // GET: ApplicationUsers/Details/5
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ApplicationUserDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUserDTO = await _context.ApplicationUsers
                .Where(u => u.Id == id)
                .Select(u => new ApplicationUserDTO(u.Id, u.Name, u.Surname, u.UserName, u.PhoneNumber))
                .FirstOrDefaultAsync();

            if (applicationUserDTO == null)
            {
                _logger.LogError($"Error: ApplicationUser with id {id} does not exist");

                return NotFound();
            }

            return Ok(applicationUserDTO);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<ApplicationUserForTeamDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetUsersForTeam(string? sportName, int? skill, int? minAge, int? maxAge)
        {
            if (minAge != null && maxAge != null && minAge > maxAge)
            {
                ModelState.AddModelError("minAge&maxAge", "minAge must be lower than or equal to maxAge");
                _logger.LogError($"{DateTime.Now} Error: minAge must be lower than or equal to maxAge");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            if (skill != null && (skill < 1 || skill > 5))
            {
                ModelState.AddModelError("skill", "skill must be between 1 and 5");
                _logger.LogError($"{DateTime.Now} Error: skill must be between 1 and 5");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            IList<ApplicationUserForTeamDTO> users = await _context.ApplicationUsers
                .Include(u => u.Interested).ThenInclude(i => i.Sport)
                .Where(u => (sportName == null && skill == null || u.Interested.Any(i => (sportName == null || i.Sport.Name == sportName)
                            && (skill == null || i.Skill >= skill)))
                        && (minAge == null || u.Age >= minAge)
                        && (maxAge == null || u.Age <= maxAge))
                .OrderBy(u => u.Surname)
                    .ThenBy(u => u.Name)
                .Select(u => new ApplicationUserForTeamDTO(u.Id, u.Name, u.Surname, u.Age,
                    u.Interested.Select(i => new InterestedInDTO(i.Sport.Name, i.Skill)).ToList()))
                .ToListAsync();

            return Ok(users);
        }

        private bool ApplicationUserDTOExists(string name, string surname)
        {
            return _context.ApplicationUsers.Any(u => u.Name == name && u.Surname == surname);
        }
    }
}
