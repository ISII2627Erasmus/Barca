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
}
 

}

