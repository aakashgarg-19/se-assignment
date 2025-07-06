using MediatR;

using RL.Backend.Dto;
using RL.Backend.Models;

namespace RL.Backend.Commands
{
    public class GetPlanProcedureUsersQuery : IRequest<ApiResponse<List<UserDto>>>
    {
        public int PlanProcedureId { get; set; }
    }
}
