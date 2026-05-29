using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    /// <summary>
    /// Модель представления команд импорта, экспорта и конвертации.
    /// </summary>
    public class ImportExportControlVM : ViewModelBase
    {
        private readonly IImportExportService _importExportService;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportControlVM" />.
        /// </summary>
        /// <param name="importExportService">Сервис импорта-экспорта.</param>
        /// <param name="repositoryExplorerControlVM">Модель представления обозревателя репозитория.</param>
        public ImportExportControlVM(
            IImportExportService importExportService,
            RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            ArgumentNullException.ThrowIfNull(importExportService);
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);

            _importExportService = importExportService;
            _repositoryExplorerControlVM = repositoryExplorerControlVM;

            RefreshAdapters();
        }

        /// <summary>
        /// Доступные адаптеры импорта-экспорта.
        /// </summary>
        public ObservableCollection<ImportExportAdapterVM> AvailableAdapters { get; } = new();

        /// <summary>
        /// Команда обновления списка доступных адаптеров.
        /// </summary>
        public RelayCommand RefreshAdaptersCommand => new(_ => RefreshAdapters());

        /// <summary>
        /// Команда экспорта рабочего дерева в PHJSON.
        /// </summary>
        public RelayCommand ExportToPhjsonCommand => DelegateCommand(_repositoryExplorerControlVM.ExportToPhjsonCommand);

        /// <summary>
        /// Команда импорта рабочего дерева из PHJSON.
        /// </summary>
        public RelayCommand ImportFromPhjsonCommand => DelegateCommand(_repositoryExplorerControlVM.ImportFromPhjsonCommand);

        /// <summary>
        /// Команда импорта рабочего дерева из Excel.
        /// </summary>
        public RelayCommand ImportTreeFromXlsxCommand => DelegateCommand(_repositoryExplorerControlVM.ImportTreeFromXlsxCommand);

        /// <summary>
        /// Команда конвертации Excel в PHJSON.
        /// </summary>
        public RelayCommand ConvertXlsxToPhjsonCommand => DelegateCommand(_repositoryExplorerControlVM.ConvertXlsxToPhjsonCommand);

        private void RefreshAdapters()
        {
            AvailableAdapters.Clear();

            foreach (var adapterInfo in _importExportService.GetAvailableAdapters())
            {
                AvailableAdapters.Add(new ImportExportAdapterVM(adapterInfo));
            }
        }

        private static RelayCommand DelegateCommand(RelayCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            return new RelayCommand(
                command.Execute,
                command.CanExecute);
        }
    }
}
