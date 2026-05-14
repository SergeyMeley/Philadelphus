using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Controls.RepositoryExplorer;
using Philadelphus.Presentation.Wpf.UI.Views.Controls.TabItemsControls.ApplicationSettingsTabItemsControls;
using Philadelphus.Presentation.Wpf.UI.Views.Controls.TabItemsControls.LaunchWindowTabItemsControls;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using Serilog;
using System.Reflection;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для стартового окна.
    /// </summary>
    public class LaunchWindowVM : ControlBaseVM
    {
        /// <summary>
        /// Стартовое окно.
        /// </summary>
        public List<LaunchWindowTabItemControlVM> LaunchWindowTabItemsVMs { get; set; }

        private LaunchWindowTabItemControlVM _selectedLaunchWindowTabItemVM;
        public LaunchWindowTabItemControlVM SelectedLaunchWindowTabItemVM
        {
            get => _selectedLaunchWindowTabItemVM;
            set
            {
                if (_selectedLaunchWindowTabItemVM != value)
                {
                    _selectedLaunchWindowTabItemVM = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly ApplicationSettingsControlVM _applicationSettingsControlVM;
        /// <summary>
        /// Настройки приложения.
        /// </summary>
        public ApplicationSettingsControlVM ApplicationSettingsControlVM { get => _applicationSettingsControlVM; }

        private DataStoragesCollectionVM _dataStoragesCollectionVM;
        /// <summary>
        /// Хранилище данных.
        /// </summary>
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesCollectionVM; }

        private PhiladelphusRepositoryCollectionVM _repositoryCollectionVM;
        /// <summary>
        /// Репозиторий.
        /// </summary>
        public PhiladelphusRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private PhiladelphusRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        /// <summary>
        /// Репозиторий.
        /// </summary>
        public PhiladelphusRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        private StorageCreationControlVM _storageCreationControlVM;
        /// <summary>
        /// Элемент управления.
        /// </summary>
        public StorageCreationControlVM StorageCreationControlVM { get => _storageCreationControlVM; }

        private RepositoryCreationControlVM _repositoryCreationVM;
        /// <summary>
        /// Репозиторий.
        /// </summary>
        public RepositoryCreationControlVM RepositoryCreationVM { get => _repositoryCreationVM; }
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get => Environment.UserName; }
        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Title { get => $"Чубушник {AssemblyVersion}"; }
        /// <summary>
        /// Выполняет операцию AssemblyVersion.
        /// </summary>
        /// <returns>Полученные данные.</returns>
        public string AssemblyVersion { get => $"v.{Assembly.GetExecutingAssembly().GetName().Version}"; }
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LaunchWindowVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="repositoryCollectionVM">Параметр repositoryCollectionVM.</param>
        /// <param name="repositoryHeadersCollectionVM">Параметр repositoryHeadersCollectionVM.</param>
        /// <param name="storageCreationControlVM">Параметр storageCreationControlVM.</param>
        /// <param name="repositoryCreationControlVM">Параметр repositoryCreationControlVM.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="applicationSettingsControlVM">Параметр applicationSettingsControlVM.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public LaunchWindowVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            PhiladelphusRepositoryCollectionVM repositoryCollectionVM,
            PhiladelphusRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            StorageCreationControlVM storageCreationControlVM,
            RepositoryCreationControlVM repositoryCreationControlVM,
            ApplicationCommandsVM applicationCommandsVM,
            ApplicationSettingsControlVM applicationSettingsControlVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);
            ArgumentNullException.ThrowIfNull(repositoryCollectionVM);
            ArgumentNullException.ThrowIfNull(repositoryHeadersCollectionVM);
            ArgumentNullException.ThrowIfNull(storageCreationControlVM);
            ArgumentNullException.ThrowIfNull(repositoryCreationControlVM);
            ArgumentNullException.ThrowIfNull(applicationSettingsControlVM);

            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;
            _repositoryHeadersCollectionVM.CheckPhiladelphusRepositoryAvailableAction = x => CheckPhiladelphusRepositoryAvailable(x);
            _storageCreationControlVM = storageCreationControlVM;
            _repositoryCreationVM = repositoryCreationControlVM;
            _applicationSettingsControlVM = applicationSettingsControlVM;

            _storageCreationControlVM.OpenConnectionStringsSettingsControlCommand = OpenConnectionStringsSettingsControlCommand;
            _repositoryCreationVM.OpenDataStoragesSettingsControlCommand = OpenDataStoragesSettingsControlCommand;

            InitializeTabs();
        }

        /// <summary>
        /// Команда выполнения операции главного окна.
        /// </summary>
        public RelayCommand OpenMainWindowCommand => _applicationCommandsVM.OpenMainWindowCommand;
        public RelayCommand OpenMainWindowWithHeaderCommand
        {
            get
            {
                return new RelayCommand(
                    obj =>
                {
                    var headerVM = RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM;
                    RepositoryCollectionVM.CurrentRepositoryVM = RepositoryCollectionVM.PhiladelphusRepositoriesVMs.FirstOrDefault(x => x.Uuid == headerVM.Uuid);

                    if (OpenMainWindowCommand.CanExecute(obj))
                        OpenMainWindowCommand.Execute(obj);
                },
                ce =>
                {
                    if (RepositoryHeadersCollectionVM == null)
                        return false;
                    if (RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM == null)
                        return false;
                    if (RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM is PhiladelphusRepositoryHeaderVM == false)
                        return false;
                    return RepositoryHeadersCollectionVM.SelectedPhiladelphusRepositoryHeaderVM.IsPhiladelphusRepositoryAvailable;
                });
                
            }
        }
        public RelayCommand OpenDataStoragesSettingsControlCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    SelectedLaunchWindowTabItemVM = LaunchWindowTabItemsVMs.Find(x => x.Content is LaunchWindowStoragesTabControl);
                });
            }
        }

        public RelayCommand OpenConnectionStringsSettingsControlCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    SelectedLaunchWindowTabItemVM = LaunchWindowTabItemsVMs.Find(x => x.Content is LaunchWindowSettingsTabControl);
                    _applicationSettingsControlVM.SelectedApplicationSettingsTabItemVM = _applicationSettingsControlVM.ApplicationSettingsTabItemsVMs.Find(x => x.Content is ConnectionStringsTabControl);
                });
            }
        }
        private bool CheckPhiladelphusRepositoryAvailable(PhiladelphusRepositoryHeaderVM header)
        {
            if (header == null)
                return false;
            if (DataStoragesSettingsVM == null)
                throw new NullReferenceException("Не инициализирована модель представления настроек хранилищ данных.");
            if (DataStoragesSettingsVM.DataStoragesVMs == null)
                throw new NullReferenceException("Не инициализированы модели представлений хранилищ данных.");

            //var dataStorage = DataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model).FirstOrDefault(x => x.Uuid == header.OwnDataStorageUuid);
            //if (dataStorage == null)
            //{
            //    //var text = $"Не найдено хранилище данных {header.OwnDataStorageName} [{header.OwnDataStorageUuid}] для заголовка репозитория {header.Name} [{header.Uuid}]";
            //    //NotificationService.SendNotification(text, NotificationCriticalLevelModel.Warning, NotificationTypesModel.TextMessage);

            //    return false;
            //}

            if (RepositoryCollectionVM.CheckPhiladelphusRepositoryVMAvailable(header.Uuid, out var PhiladelphusRepositoryVM))
            {
                if (PhiladelphusRepositoryVM != null)
                {
                    header.IsPhiladelphusRepositoryAvailable = true;
                    if (header.Name != PhiladelphusRepositoryVM.Name)
                        header.Name = PhiladelphusRepositoryVM.Name;
                    if (header.Description != PhiladelphusRepositoryVM.Description)
                        header.Description = PhiladelphusRepositoryVM.Description;
                    if (header.OwnDataStorageName != PhiladelphusRepositoryVM.OwnDataStorage.Name)
                        header.OwnDataStorageName = PhiladelphusRepositoryVM.OwnDataStorage.Name;
                    if (header.OwnDataStorageUuid != PhiladelphusRepositoryVM.OwnDataStorage.Uuid)
                        header.OwnDataStorageUuid = PhiladelphusRepositoryVM.OwnDataStorage.Uuid;
                    return true;
                }
                return false;
            }
            else
            {
                header.IsPhiladelphusRepositoryAvailable = false;
                return false;
            }
                
        }
        private void InitializeTabs()
        {
            var tab1 = _serviceProvider.GetRequiredService<LaunchWindowTabItemControlVM>();
            tab1.Header = "Главная";
            tab1.IconKey = "imageRepository";
            tab1.Content = new LaunchWindowMainTabControl() { DataContext = this };

            var tab2 = _serviceProvider.GetRequiredService<LaunchWindowTabItemControlVM>();
            tab2.Header = "Создать";
            tab2.IconKey = "imageAdd";
            tab2.Content = new LaunchWindowCreatingTabControl() { DataContext = this };

            var tab3 = _serviceProvider.GetRequiredService<LaunchWindowTabItemControlVM>();
            tab3.Header = "Открыть";
            tab3.IconKey = "imageOpen";
            tab3.Content = new LaunchWindowOpeningTabControl() { DataContext = this };

            var tab4 = _serviceProvider.GetRequiredService<LaunchWindowTabItemControlVM>();
            tab4.Header = "Хранилища";
            tab4.IconKey = "imageStorage";
            tab4.Content = new LaunchWindowStoragesTabControl() { DataContext = this };

            var tab5 = _serviceProvider.GetRequiredService<LaunchWindowTabItemControlVM>();
            tab5.Header = "Настройки";
            tab5.IconKey = "imageSettings";
            tab5.Content = new LaunchWindowSettingsTabControl() { DataContext = this };

            LaunchWindowTabItemsVMs = new List<LaunchWindowTabItemControlVM> { tab1, tab2, tab3, tab4, tab5 };

            SelectedLaunchWindowTabItemVM = LaunchWindowTabItemsVMs.FirstOrDefault(t => t.Content is LaunchWindowMainTabControl);
        }
    }
}
