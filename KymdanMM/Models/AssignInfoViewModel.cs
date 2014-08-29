using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KymdanMM.Model.Models;

namespace KymdanMM.Models
{
    public class AssignInfoViewModel : BaseModel
    {
        public int DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public int ProgressStatusId { get; set; }
        public bool Finished { get; set; }
        public int MaterialId { get; set; }
    }
}