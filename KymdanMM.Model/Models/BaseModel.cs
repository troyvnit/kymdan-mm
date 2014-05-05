using System;
using System.Threading;

namespace KymdanMM.Model.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            CreatedDate = UpdatedDate = DateTime.Now;
            CreatedUserName = UpdatedUserName = Thread.CurrentPrincipal.Identity.Name;
        }
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedUserName { get; set; }
        public string UpdatedUserName { get; set; }
    }
}
