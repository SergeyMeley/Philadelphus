using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Mapping.MainEntitiesMapping
{
    /// <summary>
    /// Профиль AutoMapper для сопоставления атрибута элемента.
    /// </summary>
    public class ElementAttributeMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ElementAttributeMappingProfile" />.
        /// </summary>
        public ElementAttributeMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<ElementAttributeModel, ElementAttribute>()
                .ValidateMemberList(MemberList.Source)

                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.CustomCode, opt => opt.MapFrom(src => src.CustomCode))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))

                .ForMember(dest => dest.OwningWorkingTreeUuid, opt => opt.MapFrom(src => src.OwningWorkingTree != null ? src.OwningWorkingTree.Uuid : Guid.Empty))

                .ForMember(dest => dest.DeclaringUuid, opt => opt.MapFrom(src => src.DeclaringUuid))
                .ForMember(dest => dest.OwnerUuid, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Uuid : Guid.Empty))
                .ForMember(dest => dest.DeclaringOwnerUuid, opt => opt.MapFrom(src => src.DeclaringOwner != null ? src.DeclaringOwner.Uuid : Guid.Empty))
                .ForMember(dest => dest.ValueTypeUuid, opt => opt.Ignore())     // Сложная логика
                .ForMember(dest => dest.ValueUuid, opt => opt.Ignore())         // Сложная логика
                .ForMember(dest => dest.IsCollectionValue, opt => opt.MapFrom(src => src.IsCollectionValue))
                .ForMember(dest => dest.ValuesUuids, opt => opt.Ignore())       // Сложная логика
                .ForMember(dest => dest.VisibilityId, opt => opt.MapFrom(src => (int)src.Visibility))
                .ForMember(dest => dest.OverrideId, opt => opt.MapFrom(src => (int)src.Override))

                .AfterMap((src, dest, ctx) =>
                {
                    dest.ValueTypeUuid = src.ValueType?.Uuid;

                    if (dest.ValueTypeUuid != null)
                    {
                        if (src.IsCollectionValue == false)
                        {
                            dest.ValuesUuids = null;

                            if (src.Value != null)
                            {
                                dest.ValueUuid = src.Value.Uuid;
                            }
                        }
                        else
                        {
                            dest.ValueUuid = null;

                            if (src.Values != null)
                            {
                                dest.ValuesUuids = src.Values.Select(x => x.Uuid).ToArray();
                            }
                        }
                    }
                });

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<ElementAttribute, ElementAttributeModel>()
                .ValidateMemberList(MemberList.Source)

                .ConstructUsing((src, ctx) =>
                {
                    var ownersByUuid = ctx.Items["OwnersByUuid"] as IReadOnlyDictionary<Guid, IAttributeOwnerModel>;
                    IAttributeOwnerModel? owner = null;
                    IAttributeOwnerModel? declaringOwner = null;
                    ownersByUuid?.TryGetValue(src.OwnerUuid, out owner);
                    ownersByUuid?.TryGetValue(src.DeclaringOwnerUuid, out declaringOwner);

                    var owningTree = ctx.Items["OwningWorkingTree"] as WorkingTreeModel;
                    var notificationService = ctx.Items[nameof(INotificationService)] as INotificationService;

                    var result = new ElementAttributeModel(
                        src.Uuid,
                        owner,
                        src.DeclaringUuid,
                        declaringOwner,
                        owningTree,
                        notificationService,
                        AttributePolicyBuilder.CreateDefault(notificationService));

                    return result;
                })

                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.CustomCode, opt => opt.MapFrom(src => src.CustomCode))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))

                .ForMember(dest => dest.ValueType, opt => opt.Ignore())     // Сложная логика
                .ForMember(dest => dest.Value, opt => opt.Ignore())         // Сложная логика  
                .ForMember(dest => dest.IsCollectionValue, opt => opt.MapFrom(src => src.IsCollectionValue))
                .ForMember(dest => dest.Values, opt => opt.Ignore())        // Сложная логика
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => (VisibilityScope)src.VisibilityId))
                .ForMember(dest => dest.Override, opt => opt.MapFrom(src => (OverrideType)src.OverrideId))

                .AfterMap((src, dest, ctx) =>
                {
                    var valueTypesByUuid = ctx.Items["ValueTypesByUuid"] as IReadOnlyDictionary<Guid, TreeNodeModel>;
                    var valuesByUuid = ctx.Items["ValuesByUuid"] as IReadOnlyDictionary<Guid, TreeLeaveModel>;

                    // ValueType
                    if (src.ValueTypeUuid != null)
                    {
                        TreeNodeModel? valueType = null;
                        valueTypesByUuid?.TryGetValue(src.ValueTypeUuid.Value, out valueType);
                        dest.ValueType = valueType;
                    }

                    // Value или Values
                    if (src.IsCollectionValue == false)
                    {
                        dest.ClearValuesCollection();

                        if (src.ValueUuid != null)
                        {
                            TreeLeaveModel? value = null;
                            valuesByUuid?.TryGetValue(src.ValueUuid.Value, out value);
                            dest.Value = value;
                        }
                    }
                    else
                    {
                        dest.Value = null;

                        dest.ClearValuesCollection();
                        if (src.ValuesUuids != null)
                        {
                            foreach (var valueUuid in src.ValuesUuids)
                            {
                                if (valuesByUuid != null && valuesByUuid.TryGetValue(valueUuid, out var value))
                                {
                                    dest.TryAddValueToValuesCollection(value);
                                }
                            }
                        }
                    }
                });
        }
    }
}
