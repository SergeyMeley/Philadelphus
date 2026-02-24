using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.IO;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class StorageCreationControlVM : ControlBaseVM
    {
        private readonly IDataStoragesService _service;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollectionConfig;
        private readonly IOptions<DataStoragesCollectionConfig> _dataStoragesCollectionConfig;
        private readonly FileInfo _configFile;

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }
        public List<ConnectionStringContainer> ConnectionStringContainers { get => _connectionStringsCollectionConfig.Value.ConnectionStringContainers; }
        public ConnectionStringContainer SelectedConnectionStringContainer { get; set; }

        private DataStoragesCollectionVM _dataStoragesCollectionVM;
        public DataStoragesCollectionVM DataStoragesCollectionVM { get => _dataStoragesCollectionVM; set => _dataStoragesCollectionVM = value; }
        public StorageCreationControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IDataStoragesService service,
            IConfigurationService configurationService,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IOptions<ApplicationSettingsConfig> options,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollectionConfig,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollectionConfig,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _service = service;
            _configurationService = configurationService;
            _connectionStringsCollectionConfig = connectionStringsCollectionConfig;
            _dataStoragesCollectionConfig = dataStoragesCollectionConfig;

            _dataStoragesCollectionVM = dataStoragesCollectionVM;

            _configFile = options.Value.StoragesConfigFullPath;
        }
        public RelayCommand CreateAndSaveDataStorageCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (string.IsNullOrEmpty(Name)
                        || SelectedConnectionStringContainer == null)
                    {
                        MessageBox.Show($"Некорректно заполнены параметры, операция не выполнена.");
                        return;
                    }
                    if (_dataStoragesCollectionConfig.Value.DataStorages.Any(x => x.Name == Name))
                    {
                        MessageBox.Show($"Хранилище '{Name}' уже существует, операция не выполнена.");
                        return;
                    }

                    var model = _service.CreateDataStorageModel(Name, Description, SelectedConnectionStringContainer);
                    var vm = new DataStorageVM(model);
                    var entity = model.ToDbEntity();
                    _dataStoragesCollectionConfig.Value.DataStorages.Add(entity);
                    _configurationService.UpdateConfigFile<DataStoragesCollectionConfig>(_configFile, _dataStoragesCollectionConfig);
                    _dataStoragesCollectionVM.DataStorageVMs.Add(vm);
                    _dataStoragesCollectionVM.SelectedDataStorageVM = vm;
                });
            }
        }

        public RelayCommand OpenConnectionStringsSettingsControlCommand { get; set; }
    }
}
