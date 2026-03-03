using AutoMapper;
using Philadelphus.Core.Domain.Entities.DTOs.ImportExportDTOs;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers;
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

            CreateMap<PhiladelphusRepositoryModel, PhiladelphusRepository>()
            .ReverseMap()
            .ConstructUsing((src, dst) =>
            {
                // Специальная логика создания
                return new PhiladelphusRepositoryModel(
                    uuid: src.Uuid,
                    dataStorage: GetDataStorage(src.OwnDataStorageUuid), //TODO: ПЕРЕДЕЛАТЬ КОСТЫЛЬ
                    dbEntity: src
                );
            });

            CreateMap<TreeRootModel, TreeRootExportDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<TreeRootExportDTO, TreeRootModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<TreeNodeModel, TreeNodeExportDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.OwningRootName, opt => opt.MapFrom(src => GetOwningRootName(src)))
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.Attributes));

            CreateMap<TreeNodeExportDTO, TreeNodeModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ParentNode, opt => opt.Ignore())
                .ForMember(dest => dest.ChildNodes, opt => opt.Ignore())
                .ForMember(dest => dest.ChildLeaves, opt => opt.Ignore());

            CreateMap<TreeLeaveModel, TreeLeaveExportDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.OwningNodeName,
                    opt => opt.MapFrom(src =>
                        src.ParentNode != null
                        ? src.ParentNode.Name
                        : "Неизвестный узел"))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.Attributes));

            CreateMap<TreeLeaveExportDTO, TreeLeaveModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                // Игнорируем сложные поля модели (аналогично TreeNode)
                .ForMember(dest => dest.ParentNode, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.AllParentsRecursive, opt => opt.Ignore());

            CreateMap<ElementAttributeModel, AttributeExportDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DataTypeNodeName,
                    opt => opt.MapFrom(src =>
                        src.ValueType != null
                        ? src.ValueType.Name
                        : "Не определён"))
                .ForMember(dest => dest.ValueLeaveName,
                    opt => opt.MapFrom(src =>
                        src.Value != null
                        ? src.Value.Name
                        : "Не задано"));

            CreateMap<AttributeExportDTO, ElementAttributeModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                // Игнорируем все сложные поля
                .ForMember(dest => dest.ValueType, opt => opt.Ignore())
                .ForMember(dest => dest.Value, opt => opt.Ignore())
                .ForMember(dest => dest.Values, opt => opt.Ignore())
                .ForMember(dest => dest.IsCollectionValue, opt => opt.Ignore())
                .ForMember(dest => dest.Visibility, opt => opt.Ignore())
                .ForMember(dest => dest.Override, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.DeclaringOwner, opt => opt.Ignore())
                .ForMember(dest => dest.DeclaringUuid, opt => opt.Ignore());
        }

        private IDataStorageModel GetDataStorage(Guid uuid)
        {
            var builder = new DataStorageBuilder()
                    .SetGeneralParameters("test", "test", uuid, InfrastructureTypes.PostgreSqlEf, isDisabled: false);
            return builder.Build();
        }

        private static string GetOwningRootName(TreeNodeModel src) =>
        src.OwningWorkingTree?.ContentRoot?.Name ?? "Неизвестный корень";
    }
}
