using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Mapping.MainEntitiesMapping
{
    public class TreeLeaveMappingProfile : Profile
    {
        public TreeLeaveMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<TreeLeaveModel, TreeLeave>()
                .ValidateMemberList(MemberList.Source)

                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.CustomCode, opt => opt.MapFrom(src => src.CustomCode))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))

                .ForMember(dest => dest.OwningWorkingTreeUuid, opt => opt.MapFrom(src => src.OwningWorkingTree != null ? src.OwningWorkingTree.Uuid : Guid.Empty))

                .ForMember(dest => dest.ParentTreeNodeUuid, opt => opt.Ignore())    // Сложная логика
                .ForMember(dest => dest.SystemBaseTypeId, opt => opt.Ignore())      // Сложная логика

                .AfterMap((src, dest, ctx) =>
                {
                    dest.ParentTreeNodeUuid = src.ParentNode?.Uuid;
                    if (src is SystemBaseTreeLeaveModel st)
                    {
                        dest.SystemBaseTypeId = (int)st.SystemBaseType;
                    }
                });

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<TreeLeave, TreeLeaveModel>()
                .ValidateMemberList(MemberList.Source)

                .ConstructUsing((src, ctx) =>
                {
                    var parent = (ctx.Items["Parents"] as IEnumerable<TreeNodeModel>).Single(x => x.Uuid == src.ParentTreeNodeUuid);
                    var owner = ctx.Items["Owner"] as WorkingTreeModel;
                    var notificationService = ctx.Items[nameof(INotificationService)] as INotificationService;
                    var propertiesPolicy = ctx.Items[nameof(IPropertiesPolicy<TreeLeaveModel>)] as IPropertiesPolicy<TreeLeaveModel>;

                    if (src.SystemBaseTypeId == 0)
                    {
                        return new TreeLeaveModel(
                            src.Uuid,
                            parent,
                            owner,
                            notificationService,
                            propertiesPolicy);
                    }
                    else
                    {
                        var type = SystemBaseTreeNodeModel.GetTypeByUuid(parent.Uuid);

                        return new SystemBaseTreeLeaveModel(
                            src.Uuid,
                            parent as SystemBaseTreeNodeModel,
                            owner,
                            type,
                            notificationService,
                            propertiesPolicy);
                    }
                })

                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sequence, opt => opt.MapFrom(src => src.Sequence))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.CustomCode, opt => opt.MapFrom(src => src.CustomCode))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden));
        }
    }
}
