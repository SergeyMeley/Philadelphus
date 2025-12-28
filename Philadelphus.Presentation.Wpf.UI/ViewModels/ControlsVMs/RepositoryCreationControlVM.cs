using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Config;
using Philadelphus.Core.Domain.Services;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class RepositoryCreationControlVM : ControlVM
    {
        private readonly ITreeRepositoryCollectionService _collectionService;
        private readonly ITreeRepositoryService _service;

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; set => _dataStoragesSettingsVM = value; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public RepositoryCreationControlVM(
            IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            ITreeRepositoryCollectionService collectionService,
            ITreeRepositoryService service,
            TreeRepositoryCollectionVM repositoryCollectionVM, 
            DataStoragesSettingsVM dataStoragesSettingsVM,
            IOptions<ApplicationSettings> options,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, logger, notificationService, applicationCommandsVM)
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
                    _service.SaveChanges(result);
                    var vm = new TreeRepositoryVM(result, _service);
                    _repositoryCollectionVM.TreeRepositoriesVMs.Add(vm);
                });
            }
        }
    }
}
