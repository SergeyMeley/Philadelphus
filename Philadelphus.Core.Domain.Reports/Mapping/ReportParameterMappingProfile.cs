using AutoMapper;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Mapping
{
    /// <summary>
    /// Профиль AutoMapper для сопоставления параметра отчета.
    /// </summary>
    public class ReportParameterMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ReportParameterMappingProfile" />.
        /// </summary>
        public ReportParameterMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<ReportParameterModel, ReportParameter>();

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<ReportParameter, ReportParameterModel>();
        }
    }
}
