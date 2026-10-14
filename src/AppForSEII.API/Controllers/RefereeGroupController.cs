namespace AppForSEII.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RefereeGroupController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefereeGroupController> _logger;

    public RefereeGroupController(
        ApplicationDbContext context,
        ILogger<RefereeGroupController> logger)
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
}
