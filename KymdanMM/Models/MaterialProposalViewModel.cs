﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KymdanMM.Model.Models;

namespace KymdanMM.Models
{
    public class MaterialProposalViewModel : BaseModel
    {
        [Required]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Mã quản lý")]
        public string ManagementCode { get; set; }
        [Required]
        [Display(Name = "Mã đề xuất")]
        public string ProposalCode { get; set; }
        [Required]
        [Display(Name = "Người đề xuất")]
        public string ProposerUserName { get; set; }
        public string ProposerDisplayName { get; set; }
        [Required]
        [Display(Name = "Đơn vị đề xuất")]
        public int ProposerDepartmentId { get; set; }
        public string ProposerDepartmentName { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        [Display(Name = "Đính kèm")]
        public string File { get; set; }
        [Display(Name = "Từ đề xuất giấy")]
        public bool FromHardProposal { get; set; }
        public bool Sent { get; set; }
        public bool Approved { get; set; }
        public DateTime MinDeadline { get; set; }
        public IList<CommentViewModel> Comments { get; set; } 
    }
}