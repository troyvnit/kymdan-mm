﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KymdanMM.Model.Models;

namespace KymdanMM.Models
{
    public class MaterialViewModel : BaseModel
    {
        [Display(Name = "Tên vật tư")]
        public string MaterialName { get; set; }
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        [Display(Name = "Số lượng")]
        public decimal Quantity { get; set; }
        [Display(Name = "Số lượng tồn kho")]
        public decimal InventoryQuantity { get; set; }
        [Display(Name = "Đơn vị")]
        public string Unit { get; set; }
        [Display(Name = "Đã sử dụng")]
        public bool Used { get; set; }
        [Display(Name = "Mục đích sử dụng")]
        public string UsingPurpose { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày đề nghị nhận")]
        public DateTime? Deadline { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày triển khai")]
        public DateTime? StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày duyệt")]
        public DateTime? ApproveDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Thời hạn hoàn thành")]
        public DateTime? FinishDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày giao")]
        public DateTime? DeliveryDate { get; set; }
        [Display(Name = "Hoàn thành")]
        public bool Finished { get; set; }
        [Display(Name = "Duyệt")]
        public bool Approved { get; set; }
        public string File { get; set; }
        [Display(Name = "Trạng thái")]
        public int ProgressStatusId { get; set; }
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        [Display(Name = "Người thực hiện")]
        public string ImplementerUserName { get; set; }
        [Display(Name = "Những người thực hiện")]
        public string ImplementerUserNames { get; set; }
        [Display(Name = "Người thực hiện")]
        public string ImplementerDisplayName { get; set; }
        [Display(Name = "Người thực hiện")]
        public string ImplementerDisplayNames { get; set; }
        [Display(Name = "Đơn vị thực hiện")]
        public int ImplementerDepartmentId { get; set; }
        [Display(Name = "Những đơn vị thực hiện")]
        public string ImplementerDepartmentIds { get; set; }
        [Display(Name = "Đơn vị thực hiện")]
        public string ImplementerDepartmentName { get; set; }
        [Display(Name = "Đơn vị thực hiện")]
        public string ImplementerDepartmentNames { get; set; }
        [Display(Name = "Đơn vị đề nghị")]
        public int ProposerDepartmentId { get; set; }
        [Display(Name = "Đơn vị đề nghị")]
        public string ProposerDepartmentName { get; set; }
        [Display(Name = "Người đề xuất")]
        public string ProposerUserName { get; set; }
        [Display(Name = "Người đề xuất")]
        public string ProposerDisplayName { get; set; }
        public int MaterialProposalId { get; set; }
        [Display(Name = "Mã đề xuất")]
        public string MaterialProposalCode { get; set; }
        public string Type { get; set; }
        public string LastProposalDeparmentComment { get; set; }
        public string LastImplementDepartmentComment { get; set; }
        public string LastGeneralManagerComment { get; set; }
        public string ProposalDeparmentComments { get; set; }
        public string ImplementDepartmentComments { get; set; }
        public string GeneralManagerComments { get; set; }
        public string LastProposalDeparmentCommentReadClass { get; set; }
        public string LastImplementDepartmentCommentReadClass { get; set; }
        public string LastGeneralManagerCommentReadClass { get; set; }
        public IList<CommentViewModel> Comments { get; set; }
        public virtual IList<AssignInfoViewModel> AssignInfoes { get; set; } 
    }
}