using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using Philadelphus.Presentation.Wpf.UI.Services.Implementations;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public class ApplicationVM : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IThemeService _themeService;

        private ApplicationCommandsVM _applicationCommandsVM;
        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }

        private DataStoragesCollectionVM _dataStoragesSettingsVM;
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private PhiladelphusRepositoryCollectionVM _repositoryCollectionVM;
        public PhiladelphusRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private PhiladelphusRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public PhiladelphusRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        public ApplicationVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IThemeService themeService,
            IOptions<ApplicationSettingsConfig> options,
            ApplicationCommandsVM applicationCommandsVM,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            PhiladelphusRepositoryCollectionVM PhiladelphusRepositoryCollectionVM,
            PhiladelphusRepositoryHeadersCollectionVM PhiladelphusRepositoryHeadersCollectionVM,
            RepositoryCreationControlVM RepositoryCreationVM)
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _themeService = themeService;
            _applicationCommandsVM = applicationCommandsVM;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = PhiladelphusRepositoryCollectionVM;
            _repositoryHeadersCollectionVM = PhiladelphusRepositoryHeadersCollectionVM;
        }

        public Array ThemeModes => Enum.GetValues(typeof(ControlsThemeMode));

        private ControlsThemeMode _selectedTheme;
        public ControlsThemeMode SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    OnPropertyChanged();
                    _themeService.ApplyTheme(value);
                }
            }
        }
    }
}
