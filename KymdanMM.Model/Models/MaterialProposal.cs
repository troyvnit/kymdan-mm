using System.Collections.Generic;

namespace KymdanMM.Model.Models
{
    public class MaterialProposal : BaseModel
    {
        public string Description { get; set; }
        public string ManagementCode { get; set; }
        public string ProposalCode { get; set; }
        public string ProposerUserName { get; set; }
        public int ProposerDepartmentId { get; set; }
        public string ImplementerUserName { get; set; }
        public int ImplementerDepartmentId { get; set; }
        public int ProgressStatusId { get; set; }
        public ProgressStatus ProgressStatus { get; set; }
        public bool Finished { get; set; }
        public ApproveStatus ApproveStatus { get; set; }
        public string Note { get; set; }
        public IList<Material> Materials { get; set; } 
    }

    public enum ApproveStatus
    {
        Unapproved, ManagerApproved, AdminApproved
    }
}
