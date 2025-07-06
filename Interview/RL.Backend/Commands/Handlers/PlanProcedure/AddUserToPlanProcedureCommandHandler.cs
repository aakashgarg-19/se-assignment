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

        public AddUserToPlanProcedureCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Unit>> Handle(AddUserToPlanProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Validate request
                if (request.PlanProcedureId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanProcedureId"));
                if (request.UserId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId"));

                var planProcedure = await _context.PlanProcedures
                    .Include(pp => pp.PlanProcedureUsers)
                    .FirstOrDefaultAsync(pp => pp.PlanProcedureId == request.PlanProcedureId);
                var user = await _context.Users.FirstOrDefaultAsync(p => p.UserId == request.UserId);

                if (planProcedure is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"PlanProcedureId: {request.PlanProcedureId} not found"));
                if (user is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {request.UserId} not found"));

                //Already has the user, so just succeed
                if (planProcedure.PlanProcedureUsers.Any(p => p.UserId == user.UserId))
                    return ApiResponse<Unit>.Succeed(new Unit());

                planProcedure.PlanProcedureUsers.Add(new PlanProcedureUser
                {
                    UserId = user.UserId,
                    PlanProcedureId = planProcedure.PlanProcedureId,
                });

                await _context.SaveChangesAsync();

                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (Exception e)
            {
                return ApiResponse<Unit>.Fail(e);
            }
        }
    }
}
