using AutoMapper;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Mapping
{
    /// <summary>
    /// Профиль AutoMapper для сопоставления отчета.
    /// </summary>
    public class ReportInfoMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ReportInfoMappingProfile" />.
        /// </summary>
        public ReportInfoMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<ReportInfoModel, ReportInfo>();

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<ReportInfo, ReportInfoModel>();
        }
    }
}
