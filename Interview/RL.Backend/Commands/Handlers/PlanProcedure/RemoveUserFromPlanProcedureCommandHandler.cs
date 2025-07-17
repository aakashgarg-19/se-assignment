using MediatR;

using Microsoft.EntityFrameworkCore;

using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.PlanProcedure
{
    public class RemoveUserFromPlanProcedureCommandHandler : IRequestHandler<RemoveUserFromPlanProcedureCommand, ApiResponse<Unit>>
    {
        private readonly RLContext _context;
        private readonly ILogger<RemoveUserFromPlanProcedureCommandHandler> _logger;

        public RemoveUserFromPlanProcedureCommandHandler(RLContext context, ILogger<RemoveUserFromPlanProcedureCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<Unit>> Handle(RemoveUserFromPlanProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.PlanProcedureId < 1)
                {
                    _logger.Log(LogLevel.Error, "Invalid PlanProcedureId: {PlanProcedureId}. Must be greater than 0.", request.PlanProcedureId);
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanProcedureId: Must be greater than 0."));
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    _logger.Log(LogLevel.Error, "Invalid UserId: Cannot be null or whitespace.");
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId: Cannot be null or whitespace."));
                }

                if (request.UserId == "*")
                {
                    var anyUsersFound = await _context.PlanProcedureUsers.AnyAsync(pu => pu.PlanProcedureId == request.PlanProcedureId, cancellationToken);

                    if (!anyUsersFound)
                    {
                        _logger.Log(LogLevel.Error, "No users found assigned to PlanProcedureId: {PlanProcedureId}. No action needed.", request.PlanProcedureId);
                        return ApiResponse<Unit>.Fail(new NotFoundException("No users found for this PlanProcedure."));
                    }
                    var planProcedureUsers = _context.PlanProcedureUsers.Where(pu => pu.PlanProcedureId == request.PlanProcedureId);

                    _context.PlanProcedureUsers.RemoveRange(planProcedureUsers);
                    _logger.Log(LogLevel.Information, "All users removed from PlanProcedureId: {PlanProcedureId}.", request.PlanProcedureId);
                }
                else
                {
                    if (!int.TryParse(request.UserId, out int userId))
                    {
                        _logger.Log(LogLevel.Error, "UserId must be an integer or '*'");
                        return ApiResponse<Unit>.Fail(new BadRequestException("UserId must be an integer or '*'"));
                    }

                    var planProcedureUser = await _context.PlanProcedureUsers
                        .FirstOrDefaultAsync(pu =>
                            pu.PlanProcedureId == request.PlanProcedureId &&
                            pu.UserId == userId,
                            cancellationToken);

                    if (planProcedureUser == null)
                    {
                        _logger.Log(LogLevel.Error, "User {UserId} not found assigned to PlanProcedureId: {PlanProcedureId}. No action needed.", userId, request.PlanProcedureId);
                        return ApiResponse<Unit>.Fail(new NotFoundException("No users found for this PlanProcedure."));
                    }

                    _context.PlanProcedureUsers.Remove(planProcedureUser);
                    _logger.Log(LogLevel.Information, "Marked user {UserId} for removal from PlanProcedureId: {PlanProcedureId}", userId, request.PlanProcedureId);
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ApiResponse<Unit>.Succeed(Unit.Value);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Operation to remove user(s) from PlanProcedureId: {PlanProcedureId} was cancelled.", request.PlanProcedureId);
                return ApiResponse<Unit>.Fail(new OperationCanceledException("The operation was cancelled."));
            }
            catch (DbUpdateException ex)
            {
                _logger.Log(LogLevel.Error, ex, "Database error occurred while removing user(s) from PlanProcedureId: {PlanProcedureId}.", request.PlanProcedureId);
                return ApiResponse<Unit>.Fail(new Exception("A database error occurred.", ex));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "An unexpected error occurred while removing user(s) from PlanProcedureId: {PlanProcedureId}.", request.PlanProcedureId);
                return ApiResponse<Unit>.Fail(ex);
            }
        }
    }
}
