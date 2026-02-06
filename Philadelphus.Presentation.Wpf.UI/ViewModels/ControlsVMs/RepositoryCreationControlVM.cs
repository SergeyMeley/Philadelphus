using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Implementations;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class RepositoryCreationControlVM : ControlBaseVM
    {
        private readonly ITreeRepositoryCollectionService _collectionService;
        private readonly ITreeRepositoryService _service;
        private readonly IConfigurationService _configurationService;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
        private readonly TreeRepositoryCollectionVM _repositoryCollectionVM;
        private readonly TreeRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;


        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }

        public DataStoragesCollectionVM DataStoragesCollectionVM { get => _dataStoragesCollectionVM; }

        public RepositoryCreationControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            ITreeRepositoryCollectionService collectionService,
            ITreeRepositoryService service,
            IConfigurationService configurationService,
            TreeRepositoryCollectionVM repositoryCollectionVM, 
            TreeRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _collectionService = collectionService;
            _service = service;
            _configurationService = configurationService;
            _dataStoragesCollectionVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;
        }
        public RelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_dataStoragesCollectionVM.SelectedDataStorageVM == null)
                        return;
                    if (_repositoryCollectionVM.TreeRepositoriesVMs.Any(x => x.Name == _name))
                    {
                        MessageBox.Show($"Репозиторий '{_name}' уже существует. Операция не выполнена.");
                        return;
                    }
                    var model = _collectionService.CreateNewTreeRepository(_dataStoragesCollectionVM.SelectedDataStorageVM.Model);
                    model.Name = _name;
                    model.Description = _description;
                    _collectionService.SaveChanges(model);      //TODO: Переделать

                    var vm = new TreeRepositoryVM(model, _service);
                    _repositoryCollectionVM.TreeRepositoriesVMs.Add(vm);
                    var headerVm = _repositoryHeadersCollectionVM.AddTreeRepositoryHeaderVMFromTreeRepositoryVM(vm);
                });
            }
        }

        public RelayCommand OpenDataStoragesSettingsControlCommand { get; set; }
    }
}
