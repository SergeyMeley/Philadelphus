using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Mapping.MainEntitiesMapping
{
    public class PhiladelphusRepositoryMappingProfile : Profile
    {
        public PhiladelphusRepositoryMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<PhiladelphusRepositoryModel, PhiladelphusRepository>()
                .ValidateMemberList(MemberList.Source)

                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))

                .ForMember(dest => dest.OwnDataStorageUuid, opt => opt.MapFrom(src => src.OwnDataStorage != null ? src.OwnDataStorage.Uuid : Guid.Empty))
                .ForMember(dest => dest.ContentWorkingTreesUuids, opt => opt.MapFrom(src => src.ContentShrub.ContentWorkingTreesUuids));

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<PhiladelphusRepository, PhiladelphusRepositoryModel>()
                .ValidateMemberList(MemberList.Source)

                .ConstructUsing((src, ctx) =>
                {
                    var storage = (ctx.Items["DataStorages"] as IEnumerable<IDataStorageModel>).Single(x => x.Uuid == src.OwnDataStorageUuid);
                    var notificationService = ctx.Items[nameof(INotificationService)] as INotificationService;
                    var propertiesPolicy = ctx.Items[nameof(IPropertiesPolicy<PhiladelphusRepositoryModel>)] as IPropertiesPolicy<PhiladelphusRepositoryModel>;
                    var srubPropertiesPolicy = ctx.Items[nameof(IPropertiesPolicy<ShrubModel>)] as IPropertiesPolicy<ShrubModel>;

                    return new PhiladelphusRepositoryModel(
                        src.Uuid,
                        storage,
                        notificationService,
                        propertiesPolicy,
                        srubPropertiesPolicy);
                })

                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))

                .ForPath(dest => dest.ContentShrub.ContentWorkingTreesUuids, opt => opt.MapFrom(src => src.ContentWorkingTreesUuids.ToList()));
        }
    }
}
