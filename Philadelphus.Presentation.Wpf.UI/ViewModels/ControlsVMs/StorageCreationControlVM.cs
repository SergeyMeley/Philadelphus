using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class StorageCreationControlVM : ControlBaseVM
    {
        private readonly IDataStoragesService _service;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollectionConfig;

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
            DataStoragesCollectionVM dataStoragesSettingsVM,
            IOptions<ApplicationSettingsConfig> options,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollectionConfig,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _service = service;
            _connectionStringsCollectionConfig = connectionStringsCollectionConfig;

            _dataStoragesCollectionVM = dataStoragesSettingsVM;
        }
        public RelayCommand CreateAndSaveDataStorageCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    MessageBox.Show("Функционал еще не реализован, добавьте хранилище путем редактирования конфигурационного файла.");
                });
            }
        }

        public RelayCommand OpenConnectionStringsSettingsControlCommand { get; set; }
    }
}
