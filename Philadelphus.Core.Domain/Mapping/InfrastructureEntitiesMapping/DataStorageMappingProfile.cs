using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Mapping.InfrastructureEntitiesMapping
{
    /// <summary>
    /// Профиль AutoMapper для сопоставления хранилища данных.
    /// </summary>
    public class DataStorageMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataStorageMappingProfile" />.
        /// </summary>
        public DataStorageMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<DataStorageModel, DataStorage>()
                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.InfrastructureType, opt => opt.MapFrom(src => src.InfrastructureType))
                .ForMember(dest => dest.HasPhiladelphusRepositoriesInfrastructureRepository, opt => opt.MapFrom(src => src.HasPhiladelphusRepositoriesInfrastructureRepository))
                .ForMember(dest => dest.HasShrubMembersInfrastructureRepository, opt => opt.MapFrom(src => src.HasShrubMembersInfrastructureRepository))
                .ForMember(dest => dest.HasReportsInfrastructureRepository, opt => opt.MapFrom(src => src.HasReportsInfrastructureRepository))
                .ForMember(dest => dest.IsHidden, opt => opt.MapFrom(src => src.IsHidden));

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<DataStorage, DataStorageModel>()
                .ConstructUsing((src, ctx) => new DataStorageModel(
                    ctx.Items["Logger"] as ILogger,
                    src.Uuid,
                    src.Name,
                    src.Description,
                    src.InfrastructureType,
                    src.IsHidden))
                .ForMember(dest => dest.PhiladelphusRepositoriesInfrastructureRepository, opt => opt.Ignore())  // Сложная логика
                .ForMember(dest => dest.ShrubMembersInfrastructureRepository, opt => opt.Ignore())              // Сложная логика
                .ForMember(dest => dest.ReportsInfrastructureRepository, opt => opt.Ignore())                   // Сложная логика
                .AfterMap((src, dest, ctx) =>
                {
                    if (dest.IsHidden == false)
                    {
                        var repositories = ctx.Items["Repositories"] as IEnumerable<IInfrastructureRepository>;

                        if (src.HasPhiladelphusRepositoriesInfrastructureRepository)
                        {
                            var repo = repositories.SingleOrDefault(x => x is IPhiladelphusRepositoriesInfrastructureRepository);
                            dest.InfrastructureRepositories.Add(InfrastructureEntityGroups.PhiladelphusRepositories, repo);
                        }
                        if (src.HasShrubMembersInfrastructureRepository)
                        {
                            var repo = repositories.SingleOrDefault(x => x is IShrubMembersInfrastructureRepository);
                            dest.InfrastructureRepositories.Add(InfrastructureEntityGroups.ShrubMembers, repo);
                        }
                        if (src.HasReportsInfrastructureRepository)
                        {
                            var repo = repositories.SingleOrDefault(x => x is IReportsInfrastructureRepository);
                            dest.InfrastructureRepositories.Add(InfrastructureEntityGroups.Reports, repo);
                        }
                    }
                    dest.CheckAvailableAsync();
                });
        }
    }
}
