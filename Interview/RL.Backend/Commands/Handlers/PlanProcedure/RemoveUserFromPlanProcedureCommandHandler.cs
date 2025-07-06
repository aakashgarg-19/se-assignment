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

        public RemoveUserFromPlanProcedureCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Unit>> Handle(RemoveUserFromPlanProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.PlanProcedureId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanProcedureId"));

                if (string.IsNullOrWhiteSpace(request.UserId))
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId"));

                if (request.UserId == "*")
                {
                    var planProcedureUsers = _context.PlanProcedureUsers.Where(pu => pu.PlanProcedureId == request.PlanProcedureId);

                    if (!planProcedureUsers.Any())
                        return ApiResponse<Unit>.Fail(new NotFoundException("No users found for this PlanProcedure."));

                    _context.PlanProcedureUsers.RemoveRange(planProcedureUsers);
                }
                else
                {
                    if (!int.TryParse(request.UserId, out int userId))
                        return ApiResponse<Unit>.Fail(new BadRequestException("UserId must be an integer or '*'"));

                    var planProcedureUser = await _context.PlanProcedureUsers
                        .FirstOrDefaultAsync(pu =>
                            pu.PlanProcedureId == request.PlanProcedureId &&
                            pu.UserId == userId,
                            cancellationToken);

                    if (planProcedureUser == null)
                        return ApiResponse<Unit>.Fail(new NotFoundException("User not assigned to this PlanProcedure."));

                    _context.PlanProcedureUsers.Remove(planProcedureUser);
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ApiResponse<Unit>.Succeed(Unit.Value);
            }
            catch (Exception ex)
            {
                return ApiResponse<Unit>.Fail(ex);
            }
        }
    }
}
