using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //// Entity → DTO
            //CreateMap<AuditInfo, AuditInfoDto>();

            //// DTO → Entity (для создания)
            //CreateMap<CreateAuditInfoDto, AuditInfo>()
            //    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            //    .ForMember(dest => dest.ContentUpdatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.ContentUpdatedBy, opt => opt.Ignore())
            //    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            //    .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

            // Модель бизнес-слоя → Entity инфраструктуры
            CreateMap<AuditInfoModel, AuditInfo>()
                .ReverseMap();

            CreateMap<TreeRepositoryModel, TreeRepository>()
            .ReverseMap()
            .ConstructUsing((src, dst) =>
            {
                // Специальная логика создания
                return new TreeRepositoryModel(
                    uuid: src.Uuid,
                    dataStorage: GetDataStorage(src.OwnDataStorageUuid), //TODO: ПЕРЕДЕЛАТЬ КОСТЫЛЬ
                    dbEntity: src
                );
            });
        }

        private IDataStorageModel GetDataStorage(Guid uuid)
        {
            var builder = new DataStorageBuilder()
                    .SetGeneralParameters("test", "test", uuid, InfrastructureTypes.PostgreSqlEf, isDisabled: false);
            return builder.Build();
        }
    }
}
