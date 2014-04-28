﻿using System;

namespace KymdanMM.Model.Models
{
    public class Material : BaseModel
    {
        public string MaterialName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public bool Used { get; set; }
        public string UsingPurpose { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ApproveDate { get; set; }
        public bool Finished { get; set; }
        public ApproveStatus ApproveStatus { get; set; }
        public int ProgressStatusId { get; set; }
        public string Note { get; set; }
        public string ImplementerUserName { get; set; }
        public int ImplementerDepartmentId { get; set; }
        public int MaterialProposalId { get; set; }
        public virtual MaterialProposal MaterialProposal { get; set; }
    }
}
