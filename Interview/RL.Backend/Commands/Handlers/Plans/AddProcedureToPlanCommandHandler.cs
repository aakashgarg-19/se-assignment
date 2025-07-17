using MediatR;

using Microsoft.EntityFrameworkCore;

using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.Plans;

public class AddProcedureToPlanCommandHandler : IRequestHandler<AddProcedureToPlanCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;
    private readonly ILogger<AddProcedureToPlanCommandHandler> _logger;

    public AddProcedureToPlanCommandHandler(RLContext context, ILogger<AddProcedureToPlanCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<Unit>> Handle(AddProcedureToPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            //Validate request
            if (request.PlanId < 1)
            {
                _logger.Log(LogLevel.Error, "Invalid PlanId: {PlanId}. Must be greater than 0.", request.PlanId);
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanId: Must be greater than 0."));
            }

            if (request.ProcedureId < 1)
            {
                _logger.Log(LogLevel.Error, "Invalid ProcedureId: {ProcedureId}. Must be greater than 0.", request.ProcedureId);
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid ProcedureId: Must be greater than 0."));
            }

            var plan = await _context.Plans
                .Include(p => p.PlanProcedures)
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId, cancellationToken);

            if (plan is null)
            {
                _logger.Log(LogLevel.Error, "Plan with ID: {PlanId} not found.", request.PlanId);
                return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));
            }

            var procedure = await _context.Procedures.FirstOrDefaultAsync(p => p.ProcedureId == request.ProcedureId);

            if (procedure is null)
            {
                _logger.Log(LogLevel.Error, "Procedure with ID: {ProcedureId} not found.", request.ProcedureId);
                return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));
            }

            //Already has the procedure, so just succeed
            if (plan.PlanProcedures.Any(p => p.ProcedureId == procedure.ProcedureId))
            {
                _logger.Log(LogLevel.Information, "Procedure {ProcedureId} is already associated with Plan {PlanId}. No action needed.", procedure.ProcedureId, plan.PlanId);
                return ApiResponse<Unit>.Succeed(new Unit());
            }

            plan.PlanProcedures.Add(new Data.DataModels.PlanProcedure
            {
                ProcedureId = procedure.ProcedureId,
                PlanId = request.PlanId
            });

            await _context.SaveChangesAsync();
            _logger.Log(LogLevel.Information, "Successfully added Procedure {ProcedureId} to Plan {PlanId}.", procedure.ProcedureId, plan.PlanId);

            return ApiResponse<Unit>.Succeed(new Unit());

        }
        catch (OperationCanceledException ex)
        {
            _logger.Log(LogLevel.Warning, ex, "Operation to add Procedure {ProcedureId} to Plan {PlanId} was cancelled.", request.ProcedureId, request.PlanId);
            return ApiResponse<Unit>.Fail(new OperationCanceledException("The operation was cancelled."));
        }
        catch (DbUpdateException ex)
        {
            _logger.Log(LogLevel.Error, ex, "Database error occurred while adding Procedure {ProcedureId} to Plan {PlanId}.", request.ProcedureId, request.PlanId);
            return ApiResponse<Unit>.Fail(new Exception("A database error occurred.", ex));
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "An unexpected error occurred while adding Procedure {ProcedureId} to Plan {PlanId}.", request.ProcedureId, request.PlanId);
            return ApiResponse<Unit>.Fail(ex);
        }
    }
}