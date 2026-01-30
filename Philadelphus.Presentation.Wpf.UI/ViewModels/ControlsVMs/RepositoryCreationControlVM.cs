using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class RepositoryCreationControlVM : ControlBaseVM
    {
        private readonly ITreeRepositoryCollectionService _collectionService;
        private readonly ITreeRepositoryService _service;

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }

        private DataStoragesCollectionVM _dataStoragesSettingsVM;
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; set => _dataStoragesSettingsVM = value; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public RepositoryCreationControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            ITreeRepositoryCollectionService collectionService,
            ITreeRepositoryService service,
            TreeRepositoryCollectionVM repositoryCollectionVM, 
            DataStoragesCollectionVM dataStoragesSettingsVM,
            IOptions<ApplicationSettingsConfig> options,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _collectionService = collectionService;
            _service = service;

            _repositoryCollectionVM = repositoryCollectionVM;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
        }
        public RelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_dataStoragesSettingsVM.SelectedDataStorageVM == null)
                        return;
                    var result = _collectionService.CreateNewTreeRepository(_dataStoragesSettingsVM.SelectedDataStorageVM.Model);
                    result.Name = _name;
                    result.Description = _description;
                    _collectionService.SaveChanges(result);
                    var vm = new TreeRepositoryVM(result, _service);
                    _repositoryCollectionVM.TreeRepositoriesVMs.Add(vm);
                });
            }
        }

        public RelayCommand OpenDataStoragesSettingsControlCommand { get; set; }
    }
}
