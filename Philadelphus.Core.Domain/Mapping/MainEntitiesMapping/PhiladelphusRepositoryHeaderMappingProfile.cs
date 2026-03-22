using AutoMapper;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Mapping.MainEntitiesMapping
{
    public class PhiladelphusRepositoryHeaderMappingProfile : Profile
    {
        public PhiladelphusRepositoryHeaderMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<PhiladelphusRepositoryHeaderModel, PhiladelphusRepositoryHeader>()
                .ValidateMemberList(MemberList.Source)

                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.OwnDataStorageName, opt => opt.MapFrom(src => src.OwnDataStorageName))
                .ForMember(dest => dest.OwnDataStorageUuid, opt => opt.MapFrom(src => src.OwnDataStorageUuid))
                .ForMember(dest => dest.LastOpening, opt => opt.MapFrom(src => src.LastOpening))
                .ForMember(dest => dest.IsFavorite, opt => opt.MapFrom(src => src.IsFavorite))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden));

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<PhiladelphusRepositoryHeader, PhiladelphusRepositoryHeaderModel>()
                .ValidateMemberList(MemberList.Source)

                .ConstructUsing((src, ctx) => new PhiladelphusRepositoryHeaderModel(
                    src.Uuid))

                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.OwnDataStorageName, opt => opt.MapFrom(src => src.OwnDataStorageName))
                .ForMember(dest => dest.OwnDataStorageUuid, opt => opt.MapFrom(src => src.OwnDataStorageUuid))
                .ForMember(dest => dest.LastOpening, opt => opt.MapFrom(src => src.LastOpening))
                .ForMember(dest => dest.IsFavorite, opt => opt.MapFrom(src => src.IsFavorite))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden));
        }
    }
}
