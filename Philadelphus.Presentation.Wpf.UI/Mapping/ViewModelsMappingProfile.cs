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
    /// <summary>
    /// Профиль AutoMapper для сопоставления ViewModelsMappingProfile.
    /// </summary>
    public class ViewModelsMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ViewModelsMappingProfile" />.
        /// </summary>
        public ViewModelsMappingProfile()
        {
            CreateMap<ConnectionStringsContainer, ConnectionStringsContainerVM>().ReverseMap();
        }
    }
}
