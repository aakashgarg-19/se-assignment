using MediatR;

using Microsoft.EntityFrameworkCore;

using RL.Backend.Dto;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.PlanProcedure
{
    public class GetPlanProcedureUsersQueryHandler : IRequestHandler<GetPlanProcedureUsersQuery, ApiResponse<List<UserDto>>>
    {
        private readonly RLContext _context;

        public GetPlanProcedureUsersQueryHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<UserDto>>> Handle(GetPlanProcedureUsersQuery request, CancellationToken cancellationToken)
        {
            //Validate request
            if (request.PlanProcedureId < 1)
                return ApiResponse<List<UserDto>>.Fail(new BadRequestException("Invalid PlanProcedureId"));

            var users = await _context.PlanProcedureUsers
                .Where(x => x.PlanProcedureId == request.PlanProcedureId)
                .Select(x => new UserDto { UserId = x.UserId, Name = x.User.Name })
                .ToListAsync();

            return ApiResponse<List<UserDto>>.Succeed(users);
        }
    }
}
