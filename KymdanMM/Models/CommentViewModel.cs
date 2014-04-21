using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KymdanMM.Model.Models;

namespace KymdanMM.Models
{
    public class CommentViewModel : BaseModel
    {
        public string Content { get; set; }
        public bool Approved { get; set; }
        public string PosterUserName { get; set; }
        public string PosterDisplayName { get; set; }
        public int MaterialProposalId { get; set; }
    }
}