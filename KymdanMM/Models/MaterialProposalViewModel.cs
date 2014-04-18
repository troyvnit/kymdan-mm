﻿using System;
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
        [Display(Name = "Người thực hiện")]
        public string ImplementerUserName { get; set; }
        public string ImplementerDisplayName { get; set; }
        [Required]
        [Display(Name = "Đơn vị đề xuất")]
        public int ProposerDepartmentId { get; set; }
        public string ProposerDepartmentName { get; set; }
        [Required]
        [Display(Name = "Đơn vị thực hiện")]
        public int ImplementerDepartmentId { get; set; }
        public string ImplementerDepartmentName { get; set; }
        [Required]
        [Display(Name = "Trạng thái tiến độ")]
        public int ProgressStatusId { get; set; }
        public string Status { get; set; }
        public bool Finished { get; set; }
        public ApproveStatus ApproveStatus { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        [Required]
        [Display(Name = "Hạn chót")]
        public string Deadline { get; set; }
    }
}