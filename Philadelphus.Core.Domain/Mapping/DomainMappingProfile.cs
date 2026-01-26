using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Core.Domain.Mapping
{
    /// <summary>
    /// Профиль маппинга для Автомаппера
    /// </summary>
    public class DomainMappingProfile : Profile
    {
        /// <summary>
        /// Профиль маппинга для Автомаппера
        /// </summary>
        public DomainMappingProfile()
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
