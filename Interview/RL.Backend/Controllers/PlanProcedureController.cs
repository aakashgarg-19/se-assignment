using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanProcedureController : ControllerBase
{
    private readonly ILogger<PlanProcedureController> _logger;
    private readonly RLContext _context;
    private readonly IMediator _mediator;

    public PlanProcedureController(ILogger<PlanProcedureController> logger, RLContext context, IMediator mediator)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [EnableQuery]
    public IEnumerable<PlanProcedure> Get()
    {
        return _context.PlanProcedures;
    }

    [HttpGet("Users")]
    public async Task<IActionResult> GetUsers([FromQuery] int PlanProcedureId, CancellationToken token)
    {
        var response = await _mediator.Send(new GetPlanProcedureUsersQuery { PlanProcedureId = PlanProcedureId }, token);
        return response.ToActionResult();
    }

    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser([FromBody] AddUserToPlanProcedureCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        return response.ToActionResult();
    }

    [HttpDelete("RemoveUser")]
    public async Task<IActionResult> RemoveUser([FromBody] RemoveUserFromPlanProcedureCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        return response.ToActionResult();
    }
}
