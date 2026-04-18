using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.ImportExport.Excel;
using Philadelphus.Core.Domain.Services.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ImportFromExcelWindow.xaml
    /// </summary>
    public partial class ImportFromExcelWindow : Window
    {
        internal enum WindowMode
        {
            ConvertToPhjson,
            ImportTreeFromXlsx
        }

        private readonly ConversionService _service;
        private readonly IServiceProvider _serviceProvider;
        private WindowMode _mode = WindowMode.ConvertToPhjson;
        private PhiladelphusRepositoryModel? _repository;
        private IPhiladelphusRepositoryService? _repositoryService;
        private Action? _refreshRepositoryView;
        private string _selectedFilePath = string.Empty;

        public ImportFromExcelWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _service = new ConversionService();
        }

        internal void InitializeForConvert(ShrubModel shrub)
        {
            _mode = WindowMode.ConvertToPhjson;
            _repository = null;
            _repositoryService = null;
            _refreshRepositoryView = null;
            ConfigureWindowAppearance();
            InitializeRootsList(shrub);
        }

        internal void InitializeForImport(
            ShrubModel shrub,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action refreshRepositoryView)
        {
            _mode = WindowMode.ImportTreeFromXlsx;
            _repository = repository;
            _repositoryService = repositoryService;
            _refreshRepositoryView = refreshRepositoryView;
            ConfigureWindowAppearance();
            InitializeRootsList(shrub);
        }

        private void InitializeRootsList(ShrubModel shrub)
        {
            var roots = _service.GetExistingRootsFromStorage(shrub);
            CmbExistingRoots.ItemsSource = roots;
            CmbExistingRoots.DisplayMemberPath = nameof(TreeRootModel.Name);
            CmbExistingRoots.SelectedValuePath = nameof(TreeRootModel.Name);

            if (roots.Count > 0)
            {
                CmbExistingRoots.SelectedIndex = 0;
            }
        }

        private void ConfigureWindowAppearance()
        {
            switch (_mode)
            {
                case WindowMode.ImportTreeFromXlsx:
                    Title = "Импорт дерева из Excel";
                    BtnMainAction.Content = "Импортировать";
                    break;
                default:
                    Title = "Конвертер Excel -> JSON";
                    BtnMainAction.Content = "Конвертировать в PHJSON";
                    break;
            }
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Выберите файл Excel"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedFilePath = dialog.FileName;
                TxtFilePath.Text = Path.GetFileName(_selectedFilePath);
                TxtRootName.Text = Path.GetFileNameWithoutExtension(_selectedFilePath);
            }
        }

        private void ChkCreateNewRoot_Checked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = true;
            CmbExistingRoots.IsEnabled = false;
        }

        private void ChkCreateNewRoot_Unchecked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = false;
            CmbExistingRoots.IsEnabled = true;
        }

        private async void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetImportParameters(out var isNewRoot, out var rootName) == false)
                return;

            try
            {
                if (_mode == WindowMode.ImportTreeFromXlsx)
                {
                    if (_repository == null || _repositoryService == null)
                    {
                        MessageBox.Show("Не инициализирован контекст активного репозитория.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var progressWindow = _serviceProvider.GetRequiredService<ImportProgressWindow>();
                    progressWindow.Initialize("Импорт дерева из Excel", "Подготовка операции...");
                    progressWindow.Show();

                    var filePath = _selectedFilePath;
                    var repository = _repository;
                    var repositoryService = _repositoryService;

                    Close();

                    IProgress<string> statusProgress = new Progress<string>(status => progressWindow.UpdateStatus(status));

                    try
                    {
                        await Task.Run(() =>
                        {
                            statusProgress.Report("Чтение Excel и формирование PHJSON...");
                            var jsonResult = _service.ProcessFile(filePath, isNewRoot, rootName);

                            statusProgress.Report("Импорт дерева в Чубушник...");
                            JsonImportExportHelper.ParseJson(jsonResult, repositoryService, repository);
                        });

                        _refreshRepositoryView?.Invoke();
                        progressWindow.Complete("Импорт завершен. Сохраните репозиторий для записи в хранилище.");
                    }
                    catch (Exception ex)
                    {
                        progressWindow.Fail($"Ошибка импорта: {ex.Message}");
                    }

                    return;
                }

                SetBusyState(true);
                var jsonResult = _service.ProcessFile(_selectedFilePath, isNewRoot, rootName);

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PHJSON Files|*.phjson",
                    FileName = Path.GetFileNameWithoutExtension(_selectedFilePath) + ".phjson"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, jsonResult);
                    SetBusyState(false);
                    MessageBox.Show("Файл успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
                else
                {
                    SetBusyState(false);
                }
            }
            catch (Exception ex)
            {
                SetBusyState(false);
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetBusyState(bool isBusy)
        {
            BtnMainAction.IsEnabled = !isBusy;
            ChkCreateNewRoot.IsEnabled = !isBusy;
            TxtRootName.IsEnabled = !isBusy && ChkCreateNewRoot.IsChecked == true;
            CmbExistingRoots.IsEnabled = !isBusy && ChkCreateNewRoot.IsChecked != true;
            ProgressOperation.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
            Mouse.OverrideCursor = isBusy ? Cursors.Wait : null;
        }

        private bool TryGetImportParameters(out bool isNewRoot, out string rootName)
        {
            isNewRoot = ChkCreateNewRoot.IsChecked == true;
            rootName = string.Empty;

            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                MessageBox.Show("Сначала выберите файл Excel!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            rootName = isNewRoot
                ? TxtRootName.Text?.Trim() ?? string.Empty
                : (CmbExistingRoots.SelectedItem as TreeRootModel)?.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(rootName))
            {
                MessageBox.Show("Укажите наименование корня!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
