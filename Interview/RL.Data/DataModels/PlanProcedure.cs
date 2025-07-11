using System.ComponentModel.DataAnnotations;

using RL.Data.DataModels.Common;

namespace RL.Data.DataModels;

public class PlanProcedure : IChangeTrackable
{
    [Key]
    public int PlanProcedureId { get; set; }
    public int ProcedureId { get; set; }
    public int PlanId { get; set; }
    public virtual Procedure Procedure { get; set; }
    public virtual Plan Plan { get; set; }
    public PlanProcedure() => PlanProcedureUsers = new List<PlanProcedureUser>();
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }

    public virtual ICollection<PlanProcedureUser> PlanProcedureUsers { get; set; }
}
