using System;
using System.Globalization;
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
            Mapper.CreateMap<MaterialProposalViewModel, MaterialProposal>();
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
            Mapper.CreateMap<MaterialProposal, MaterialProposalViewModel>();
            Mapper.CreateMap<Material, MaterialViewModel>();
            Mapper.CreateMap<Comment, CommentViewModel>();
        }
    }
}