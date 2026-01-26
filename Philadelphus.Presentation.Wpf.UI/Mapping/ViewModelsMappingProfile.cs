using AutoMapper;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Mapping
{
    public class ViewModelsMappingProfile : Profile
    {
        public ViewModelsMappingProfile()
        {
            CreateMap<ConnectionStringContainer, ConnectionStringContainerVM>().ReverseMap();
        }
    }
}
