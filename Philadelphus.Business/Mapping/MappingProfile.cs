using AutoMapper;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Philadelphus.Business.Mapping
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
                    guid: src.Guid,
                    dataStorage: GetDataStorage(src.OwnDataStorageGuid) //TODO: ПЕРЕДЕЛАТЬ КОСТЫЛЬ
                );
            });
        }

        private IDataStorageModel GetDataStorage(Guid guid)
        {
            var builder = new DataStorageBuilder()
                    .SetGeneralParameters("test", "test", guid, InfrastructureEntities.Enums.InfrastructureTypes.PostgreSqlEf);
            return builder.Build();
        }
    }
}
