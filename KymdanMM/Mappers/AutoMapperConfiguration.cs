using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using AutoMapper;
using KymdanMM.Model.Models;
using KymdanMM.Models;

namespace KymdanMM.Mappers
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfile>();
                x.AddProfile<ViewModelToDomainMappingProfile>();
            });
        }
    }

    public class ViewModelToDomainMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ViewModelToDomainMappings"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<MaterialProposalViewModel, MaterialProposal>()
                .ForMember(a => a.ProposalCode, o => o.MapFrom(a => a.ProposalCode.ToUpper()))
                .ForMember(a => a.ManagementCode, o => o.MapFrom(a => a.ManagementCode.ToUpper()));
            Mapper.CreateMap<MaterialViewModel, Material>();
            Mapper.CreateMap<CommentViewModel, Comment>();
        }
    }

    public class DomainToViewModelMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "DomainToViewModelMappings"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<MaterialProposal, MaterialProposalViewModel>().ForMember(a => a.Approved, o => o.MapFrom(a => a.Materials.Count(m => !m.Approved) == 0))
                .ForMember(a => a.MinDeadline, o => o.MapFrom(a => a.Materials.Min(m => m.Deadline) != null ? ((DateTime)a.Materials.Min(m => m.Deadline)).Date : DateTime.Now.Date))
                .ForMember(a => a.HaveNewComment, o => o.MapFrom(a => a.Materials.Count(m => m.Comments.Count(c => !c.ReadUserNames.Contains(Thread.CurrentPrincipal.Identity.Name)) > 0) > 0))
                .ForMember(a => a.CreatedDate, o => o.MapFrom(a => a.CreatedDate != null ? a.CreatedDate.Date : DateTime.Now.Date));
            Mapper.CreateMap<Material, MaterialViewModel>().ForMember(a => a.MaterialProposalCode, o => o.MapFrom(a => a.MaterialProposal.ProposalCode))
                .ForMember(a => a.ProposerDepartmentId, o => o.MapFrom(a => a.MaterialProposal.ProposerDepartmentId))
                .ForMember(a => a.ApproveDate, o => o.MapFrom(a => a.ApproveDate != null ? ((DateTime)a.ApproveDate).Date : DateTime.Now.Date))
                .ForMember(a => a.Deadline, o => o.MapFrom(a => a.Deadline != null ? ((DateTime)a.Deadline).Date : DateTime.Now.Date))
                .ForMember(a => a.StartDate, o => o.MapFrom(a => a.StartDate != null ? ((DateTime)a.StartDate).Date : DateTime.Now.Date))
                .ForMember(a => a.FinishDate, o => o.MapFrom(a => a.FinishDate != null ? ((DateTime)a.FinishDate).Date : DateTime.Now.Date));
            Mapper.CreateMap<Comment, CommentViewModel>();
        }
    }
}