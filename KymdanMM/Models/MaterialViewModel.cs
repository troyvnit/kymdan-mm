using System;
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
        public int Quantity { get; set; }
        [Display(Name = "Số lượng tồn kho")]
        public int InventoryQuantity { get; set; }
        [Display(Name = "Đơn vị")]
        public string Unit { get; set; }
        [Display(Name = "Đã sử dụng")]
        public bool Used { get; set; }
        [Display(Name = "Mục đích sử dụng")]
        public string UsingPurpose { get; set; }
        [Display(Name = "Hạn chót")]
        public DateTime? Deadline { get; set; }
        [Display(Name = "Ngày bắt đầu")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "Ngày duyệt")]
        public DateTime? ApproveDate { get; set; }
        [Display(Name = "Ngày hoàn thành")]
        public DateTime? FinishDate { get; set; }
        [Display(Name = "Ngày giao")]
        public DateTime? DeliveryDate { get; set; }
        [Display(Name = "Hoàn tất")]
        public bool Finished { get; set; }
        [Display(Name = "Duyệt")]
        public bool Approved { get; set; }
        [Display(Name = "Trạng thái")]
        public int ProgressStatusId { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        [Display(Name = "Người thực hiện")]
        public string ImplementerUserName { get; set; }
        [Display(Name = "Đơn vị thực hiện")]
        public int ImplementerDepartmentId { get; set; }
        public int ProposerDepartmentId { get; set; }
        public int MaterialProposalId { get; set; }
        public string MaterialProposalCode { get; set; }
        public string Type { get; set; }
        public IList<CommentViewModel> Comments { get; set; } 
    }
}