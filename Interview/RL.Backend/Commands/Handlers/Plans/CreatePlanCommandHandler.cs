using MediatR;

using Microsoft.EntityFrameworkCore;

using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans;

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, ApiResponse<Plan>>
{
    private readonly RLContext _context;
    private readonly ILogger<CreatePlanCommandHandler> _logger;

    public CreatePlanCommandHandler(RLContext context, ILogger<CreatePlanCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<Plan>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var plan = new Plan();

            _context.Plans.Add(plan);
            _logger.Log(LogLevel.Information, "Plan entity created and marked for addition to the database.");

            await _context.SaveChangesAsync(cancellationToken);
            _logger.Log(LogLevel.Information, "Successfully created Plan with ID: {PlanId}.", plan.PlanId);

            return ApiResponse<Plan>.Succeed(plan);
        }
        catch (OperationCanceledException ex)
        {
            _logger.Log(LogLevel.Warning, ex, "Operation to create a new Plan was cancelled.");
            return ApiResponse<Plan>.Fail(new OperationCanceledException("The operation to create a plan was cancelled."));
        }
        catch (DbUpdateException ex)
        {
            _logger.Log(LogLevel.Error, ex, "Database error occurred while creating a new Plan.");
            return ApiResponse<Plan>.Fail(new Exception("A database error occurred while creating the plan.", ex));
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "An unexpected error occurred while creating a new Plan.");
            return ApiResponse<Plan>.Fail(ex);
        }
    }
}