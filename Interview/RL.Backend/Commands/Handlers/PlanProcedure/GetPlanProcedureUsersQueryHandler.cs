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
        private readonly ILogger<GetPlanProcedureUsersQueryHandler> _logger;

        public GetPlanProcedureUsersQueryHandler(RLContext context, ILogger<GetPlanProcedureUsersQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<UserDto>>> Handle(GetPlanProcedureUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                //Validate request
                if (request == null || request?.PlanProcedureId < 1)
                {
                    _logger.Log(LogLevel.Error, "Invalid PlanProcedureId: " + request?.PlanProcedureId);
                    return ApiResponse<List<UserDto>>.Fail(new BadRequestException("Invalid PlanProcedureId"));
                }

                var users = await _context.PlanProcedureUsers
                    .AsNoTracking()
                    .Where(x => x.PlanProcedureId == request.PlanProcedureId)
                    .Join(_context.Users,
                        ppu => ppu.UserId,
                        u => u.UserId,
                        (ppu, u) => new UserDto { UserId = ppu.UserId, Name = u.Name })
                    .ToListAsync(cancellationToken);

                _logger.Log(LogLevel.Information, "Successfully retrieved {UserCount} users for PlanProcedureId: " + request?.PlanProcedureId, users.Count);
                return ApiResponse<List<UserDto>>.Succeed(users);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Log(LogLevel.Error, ex, "GetPlanProcedureUsers operation was cancelled.");
                return ApiResponse<List<UserDto>>.Fail(new Exception("GetPlanProcedureUsers operation was cancelled"));
            }
            catch (DbUpdateException ex)
            {
                _logger.Log(LogLevel.Error, ex, "Database error while fetching users for PlanProcedureId: " + request?.PlanProcedureId);
                return ApiResponse<List<UserDto>>.Fail(new Exception("A database error occurred while retrieving users"));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Unexpected error while fetching user for given PlanProcedureId: " + request?.PlanProcedureId);
                return ApiResponse<List<UserDto>>.Fail(new Exception("An unexpected error occurred"));
            }
        }
    }
}
