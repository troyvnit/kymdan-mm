using System;
using System.Collections.Generic;

namespace KymdanMM.Model.Models
{
    public class MaterialProposal : BaseModel
    {
        public MaterialProposal()
        {
            Comments = new List<Comment>();
            Materials = new List<Material>();
        }
        public string Description { get; set; }
        public string ManagementCode { get; set; }
        public string ProposalCode { get; set; }
        public string ProposerUserName { get; set; }
        public int ProposerDepartmentId { get; set; }
        public string Note { get; set; }
        public bool FromHardProposal { get; set; }
        public virtual IList<Material> Materials { get; set; }
        public virtual IList<Comment> Comments { get; set; } 
    }

    public enum ApproveStatus
    {
        Unapproved, ManagerApproved, GeneralManagerApproved
    }
}
