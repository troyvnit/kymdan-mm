﻿using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "Người đề xuất")]
        public string ProposerUserName { get; set; }
        [Required]
        [Display(Name = "Người thực hiện")]
        public string ImplementerUserName { get; set; }
        [Required]
        [Display(Name = "Đơn vị đề xuất")]
        public Department Department { get; set; }
        public ProgressStatus ProgressStatus { get; set; }
        public bool Finished { get; set; }
        public bool Approved { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
    }
}