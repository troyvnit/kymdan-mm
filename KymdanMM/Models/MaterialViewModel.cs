using System;
using KymdanMM.Model.Models;

namespace KymdanMM.Models
{
    public class MaterialViewModel : BaseModel
    {
        public string MaterialName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public bool Used { get; set; }
        public string UsingPurpose { get; set; }
        public DateTime Deadline { get; set; }
        public string Note { get; set; }
        public string ImplementerUserName { get; set; }
        public int ImplementerDepartmentId { get; set; }
    }
}