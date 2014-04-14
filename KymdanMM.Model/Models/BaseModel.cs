using System;

namespace KymdanMM.Model.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            CreatedDate = DateTime.Now;
        }
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public enum Department
    {
        Material, Bussiness
    }

    public enum ProgressStatus
    {
        Progressing
    }
}
