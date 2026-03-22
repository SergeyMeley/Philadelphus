using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Mapping.MainEntitiesMapping
{
    public class WorkingTreeMappingProfile : Profile
    {
        public WorkingTreeMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<WorkingTreeModel, WorkingTree>()
                .ValidateMemberList(MemberList.Source)

                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))

                .ForMember(dest => dest.OwnDataStorageUuid, opt => opt.MapFrom(src => src.OwnDataStorage != null ? src.OwnDataStorage.Uuid : Guid.Empty));

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<WorkingTree, WorkingTreeModel>()
                .ValidateMemberList(MemberList.Source)

                .ConstructUsing((src, ctx) =>
                {
                    var storage = (ctx.Items["DataStorages"] as IEnumerable<IDataStorageModel>).Single(x => x.Uuid == src.OwnDataStorageUuid);
                    var owner = ctx.Items["Owner"] as ShrubModel;

                    return new WorkingTreeModel(
                        src.Uuid,
                        storage,
                        owner);
                })

                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden));
        }
    }
}
