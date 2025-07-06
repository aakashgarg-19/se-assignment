using RL.Data.DataModels.Common;

namespace RL.Data.DataModels;
public class PlanProcedureUser : IChangeTrackable
{
    public int PlanProcedureId { get; set; }
    public int UserId { get; set; }
    public virtual PlanProcedure PlanProcedure { get; set; }
    public virtual User User { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
}
