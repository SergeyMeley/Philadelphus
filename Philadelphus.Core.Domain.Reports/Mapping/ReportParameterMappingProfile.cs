using AutoMapper;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Mapping
{
    public class ReportParameterMappingProfile : Profile
    {
        public ReportParameterMappingProfile()
        {
            // Модель бизнес-слоя => Сущность инфраструктуры
            CreateMap<ReportParameterModel, ReportParameter>();

            // Сущность инфраструктуры => Модель бизнес-слоя
            CreateMap<ReportParameter, ReportParameterModel>();
        }
    }
}
