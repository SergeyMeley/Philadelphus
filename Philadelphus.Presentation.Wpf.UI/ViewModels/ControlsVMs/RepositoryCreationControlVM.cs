using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.IO;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class RepositoryCreationControlVM : ControlBaseVM
    {
        private readonly IPhiladelphusRepositoryCollectionService _collectionService;
        private readonly IPhiladelphusRepositoryService _repositoryService;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<PhiladelphusRepositoryHeadersCollectionConfig> _headersCollectionConfig;
        private readonly FileInfo _configFile;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
        private readonly PhiladelphusRepositoryCollectionVM _repositoryCollectionVM;
        private readonly PhiladelphusRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;

        private string _name;
        private string _description;
        
        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public DataStoragesCollectionVM DataStoragesCollectionVM { get => _dataStoragesCollectionVM; }

        public RepositoryCreationControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryCollectionService collectionService,
            IPhiladelphusRepositoryService repositoryService,
            IConfigurationService configurationService,
            PhiladelphusRepositoryCollectionVM repositoryCollectionVM, 
            PhiladelphusRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            DataStoragesCollectionVM dataStoragesSettingsVM, 
            IOptions<ApplicationSettingsConfig> options,
            IOptions<PhiladelphusRepositoryHeadersCollectionConfig> headersCollectionConfig,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _collectionService = collectionService;
            _repositoryService = repositoryService;
            _configurationService = configurationService;
            _headersCollectionConfig = headersCollectionConfig;

            _dataStoragesCollectionVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;

            _configFile = options.Value.RepositoryHeadersConfigFullPath;

            if (_dataStoragesCollectionVM.DataStoragesVMs?.Contains(_dataStoragesCollectionVM?.MainDataStorageVM) ?? false)
            {
                _dataStoragesCollectionVM.SelectedDataStorageVM = _dataStoragesCollectionVM.MainDataStorageVM;
            }
            Name = "Новый репозиторий";
        }
        public RelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (string.IsNullOrEmpty(Name)
                        || _dataStoragesCollectionVM.SelectedDataStorageVM == null)
                    {
                        MessageBox.Show($"Некорректно заполнены параметры, операция не выполнена.");
                        return;
                    }
                    if (_repositoryCollectionVM.PhiladelphusRepositoriesVMs.Any(x => x.Name == _name))
                    {
                        MessageBox.Show($"Репозиторий '{_name}' уже существует, операция не выполнена.");
                        return;
                    }

                    var model = _collectionService.CreateNewPhiladelphusRepository(_dataStoragesCollectionVM.SelectedDataStorageVM.Model);
                    model.Name = Name;
                    model.Description = Description;
                    _collectionService.SaveChanges(ref model);

                    var vm = new PhiladelphusRepositoryVM(model, _dataStoragesCollectionVM, _repositoryService);
                    
                    _repositoryCollectionVM.PhiladelphusRepositoriesVMs.Add(vm);
                    _repositoryCollectionVM.CurrentRepositoryVM = vm;

                    var headerVm = _repositoryHeadersCollectionVM.AddPhiladelphusRepositoryHeaderVMFromPhiladelphusRepositoryVM(vm);
                });
            }
        }

        public RelayCommand OpenDataStoragesSettingsControlCommand { get; set; }
    }
}
