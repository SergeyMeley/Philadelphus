using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Mapping.MainEntitiesMapping
{
    /// <summary>
    /// Профиль AutoMapper для сопоставления репозитория Чубушника.
    /// </summary>
    public class PhiladelphusRepositoryMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhiladelphusRepositoryMappingProfile" />.
        /// </summary>
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
                .ForMember(dest => dest.AvailableDataStorageUuids, opt => opt.MapFrom(src => src.DataStorages.Select(x => x.Uuid).ToArray()))
                .ForMember(dest => dest.DefaultDataStorageUuids, opt => opt.MapFrom(src => GetDefaultDataStorageUuids(src)))
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
                        srubPropertiesPolicy,
                        initializeDefaultDataStorages: false);
                })

                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden))
                .ForMember(dest => dest.DefaultShrubMembersDataStorage, opt => opt.Ignore())
                .ForMember(dest => dest.DefaultReportsDataStorage, opt => opt.Ignore())
                .ForSourceMember(src => src.AvailableDataStorageUuids, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.DefaultDataStorageUuids, opt => opt.DoNotValidate())

                .ForPath(dest => dest.ContentShrub.ContentWorkingTreesUuids, opt => opt.MapFrom(src => src.ContentWorkingTreesUuids.ToList()))
                .AfterMap((src, dest, ctx) =>
                {
                    var dataStorages = ctx.Items["DataStorages"] as IEnumerable<IDataStorageModel>
                        ?? Enumerable.Empty<IDataStorageModel>();

                    foreach (var uuid in src.AvailableDataStorageUuids ?? Array.Empty<Guid>())
                    {
                        var dataStorage = dataStorages.SingleOrDefault(x => x.Uuid == uuid);
                        if (dataStorage != null && dest.DataStorages.All(x => x.Uuid != uuid))
                            dest.DataStorages.Add(dataStorage);
                    }

                    dest.DefaultShrubMembersDataStorage = GetDefaultDataStorage(
                        src.DefaultDataStorageUuids,
                        InfrastructureEntityGroups.ShrubMembers,
                        dest.DataStorages);
                    dest.DefaultReportsDataStorage = GetDefaultDataStorage(
                        src.DefaultDataStorageUuids,
                        InfrastructureEntityGroups.Reports,
                        dest.DataStorages);
                });
        }

        private static Dictionary<InfrastructureEntityGroups, Guid> GetDefaultDataStorageUuids(
            PhiladelphusRepositoryModel repository)
        {
            var result = new Dictionary<InfrastructureEntityGroups, Guid>();
            if (repository.DefaultShrubMembersDataStorage != null)
            {
                result[InfrastructureEntityGroups.ShrubMembers] =
                    repository.DefaultShrubMembersDataStorage.Uuid;
            }
            if (repository.DefaultReportsDataStorage != null)
            {
                result[InfrastructureEntityGroups.Reports] =
                    repository.DefaultReportsDataStorage.Uuid;
            }

            return result;
        }

        private static IDataStorageModel? GetDefaultDataStorage(
            Dictionary<InfrastructureEntityGroups, Guid>? defaultDataStorageUuids,
            InfrastructureEntityGroups entityGroup,
            IEnumerable<IDataStorageModel> availableDataStorages)
        {
            if (defaultDataStorageUuids?.TryGetValue(entityGroup, out var uuid) != true)
                return null;

            return availableDataStorages.SingleOrDefault(x => x.Uuid == uuid);
        }
    }
}
