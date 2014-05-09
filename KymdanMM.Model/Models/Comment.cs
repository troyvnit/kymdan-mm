using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KymdanMM.Model.Models
{
    public class Comment : BaseModel
    {
        public string Content { get; set; }
        public bool Approved { get; set; }
        public string PosterUserName { get; set; }
        public string PosterDisplayName { get; set; }
        public string ReadUserNames { get; set; }
        public int MaterialId { get; set; }
        public virtual Material Material { get; set; }
    }
}
