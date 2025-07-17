using MediatR;

using Microsoft.EntityFrameworkCore;

using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.PlanProcedure
{
    public class AddUserToPlanProcedureCommandHandler : IRequestHandler<AddUserToPlanProcedureCommand, ApiResponse<Unit>>
    {
        private readonly RLContext _context;
        private readonly ILogger<AddUserToPlanProcedureCommandHandler> _logger;

        public AddUserToPlanProcedureCommandHandler(RLContext context, ILogger<AddUserToPlanProcedureCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<Unit>> Handle(AddUserToPlanProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Validate request
                if (request.PlanProcedureId < 1)
                {
                    _logger.Log(LogLevel.Error, "Invalid PlanProcedureId: " + request.PlanProcedureId);
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanProcedureId"));
                }

                if (request.UserId < 1)
                {
                    _logger.Log(LogLevel.Error, "Invalid UserId: " + request.UserId);
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId"));
                }

                var planProcedure = await _context.PlanProcedures.Include(pp => pp.PlanProcedureUsers).FirstOrDefaultAsync(pp => pp.PlanProcedureId == request.PlanProcedureId, cancellationToken);

                if (planProcedure is null)
                {
                    _logger.Log(LogLevel.Error, "PlanProcedure with ID: {PlanProcedureId} not found.", request.PlanProcedureId);
                    return ApiResponse<Unit>.Fail(new NotFoundException($"PlanProcedureId: {request.PlanProcedureId} not found"));
                }

                var user = await _context.Users.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

                if (user is null)
                {
                    _logger.Log(LogLevel.Error, "User with ID: {UserId} not found.", request.UserId);
                    return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {request.UserId} not found"));
                }

                //Already has the user, so just succeed
                if (planProcedure.PlanProcedureUsers.Any(p => p.UserId == user.UserId))
                    return ApiResponse<Unit>.Succeed(new Unit());

                planProcedure.PlanProcedureUsers.Add(new PlanProcedureUser
                {
                    UserId = user.UserId,
                    PlanProcedureId = planProcedure.PlanProcedureId,
                });

                await _context.SaveChangesAsync(cancellationToken);
                _logger.Log(LogLevel.Information, "User {UserId} successfully associated with PlanProcedure: " + planProcedure.PlanProcedureId, user.UserId);
                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (OperationCanceledException ex)
            {
                _logger.Log(LogLevel.Error, ex, "AddUserToPlanProcedure operation was cancelled.");
                return ApiResponse<Unit>.Fail(new Exception("AddUserToPlanProcedure operation was cancelled"));
            }
            catch (DbUpdateException ex)
            {
                _logger.Log(LogLevel.Error, ex, "Database error while adding UserId {UserId} to PlanProcedureId: " + request.PlanProcedureId, request.UserId);
                return ApiResponse<Unit>.Fail(new Exception("A database error occurred while adding the user"));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Unexpected error while adding UserId {UserId} to PlanProcedureId: " + request.PlanProcedureId, request.UserId);
                return ApiResponse<Unit>.Fail(new Exception("An unexpected error occurred"));
            }
        }
    }
}
