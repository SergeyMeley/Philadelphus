using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    /// <summary>
    /// Координирует WPF-предпросмотр и импорт данных из Excel.
    /// </summary>
    public class ExcelImportPresentationPipeline
    {
        private readonly ExcelImportPipeline _excelImportPipeline;
        private readonly ExcelImportRepositoryPreviewBuilder _repositoryPreviewBuilder;
        private readonly IPhiladelphusRepositoryCollectionService _repositoryCollectionService;
        private readonly IPhiladelphusRepositoryService _repositoryService;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVm;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportPresentationPipeline" />.
        /// </summary>
        /// <param name="excelImportPipeline">Инфраструктурный пайплайн Excel-импорта.</param>
        /// <param name="repositoryPreviewBuilder">Построитель предпросмотра репозитория.</param>
        /// <param name="repositoryCollectionService">Сервис коллекции репозиториев.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="dataStoragesCollectionVm">Модель представления хранилищ данных.</param>
        public ExcelImportPresentationPipeline(
            ExcelImportPipeline excelImportPipeline,
            ExcelImportRepositoryPreviewBuilder repositoryPreviewBuilder,
            IPhiladelphusRepositoryCollectionService repositoryCollectionService,
            IPhiladelphusRepositoryService repositoryService,
            DataStoragesCollectionVM dataStoragesCollectionVm)
        {
            ArgumentNullException.ThrowIfNull(excelImportPipeline);
            ArgumentNullException.ThrowIfNull(repositoryPreviewBuilder);
            ArgumentNullException.ThrowIfNull(repositoryCollectionService);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVm);

            _excelImportPipeline = excelImportPipeline;
            _repositoryPreviewBuilder = repositoryPreviewBuilder;
            _repositoryCollectionService = repositoryCollectionService;
            _repositoryService = repositoryService;
            _dataStoragesCollectionVm = dataStoragesCollectionVm;
        }

        /// <summary>
        /// Возвращает профили Excel, включенные в выполнение импорта.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Коллекция профилей, включенных в импорт.</returns>
        public List<ExcelImportProfile> GetProfilesForExecution(ExcelImportSchema schema)
        {
            return _excelImportPipeline.GetProfilesForExecution(schema);
        }

        /// <summary>
        /// Проверяет схему импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Результат проверки схемы.</returns>
        public ExcelImportValidationResult Validate(ExcelImportSchema schema)
        {
            return _excelImportPipeline.Validate(schema);
        }

        /// <summary>
        /// Формирует JSON-представление результата импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>JSON-представление результата импорта Excel.</returns>
        public string BuildJson(ExcelImportSchema schema)
        {
            return _excelImportPipeline.BuildJson(schema);
        }

        /// <summary>
        /// Строит модель предпросмотра репозитория по схеме импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <param name="targetExistingRootName">Имя существующего корня, в который планируется импорт.</param>
        /// <returns>Модель предпросмотра обозревателя репозитория.</returns>
        public RepositoryExplorerControlVM BuildRepositoryPreview(
            ExcelImportSchema schema,
            string? targetExistingRootName)
        {
            var importData = _excelImportPipeline.BuildImportData(schema);
            var previewRepository = CreatePreviewRepository();
            _repositoryService.GetShrubContent(previewRepository);

            var previewTree = _excelImportPipeline.ImportPreparedData(importData, previewRepository, _repositoryService);
            return _repositoryPreviewBuilder.Build(previewRepository, previewTree, targetExistingRootName);
        }

        /// <summary>
        /// Формирует краткое описание результата предпросмотра.
        /// </summary>
        /// <param name="previewViewModel">Модель предпросмотра обозревателя репозитория.</param>
        /// <returns>Краткое описание результата предпросмотра.</returns>
        public string BuildPreviewSummary(RepositoryExplorerControlVM previewViewModel)
        {
            return _repositoryPreviewBuilder.BuildSummary(previewViewModel);
        }

        /// <summary>
        /// Импортирует данные Excel в репозиторий.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="existingRoot">Существующий корень, в который выполняется импорт.</param>
        public void ImportToRepository(
            ExcelImportSchema schema,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            TreeRootModel? existingRoot)
        {
            _excelImportPipeline.ImportToRepository(schema, repository, repositoryService, existingRoot);
        }

        /// <summary>
        /// Проверяет, что в схеме есть хотя бы один включенный лист.
        /// </summary>
        /// <param name="schema">Схема импорта.</param>
        /// <param name="emptySelectionMessage">Сообщение об ошибке для пустого набора листов.</param>
        public static void EnsureHasEnabledSheets(ExcelImportSchema schema, string emptySelectionMessage)
        {
            ExcelImportPipeline.EnsureHasEnabledSheets(schema, emptySelectionMessage);
        }

        private PhiladelphusRepositoryModel CreatePreviewRepository()
        {
            var previewStorage = _dataStoragesCollectionVm.MainDataStorageVM?.Model
                ?? _dataStoragesCollectionVm.DataStoragesVMs?.Select(x => x.Model).FirstOrDefault(x => x != null);

            if (previewStorage == null)
            {
                throw new InvalidOperationException("Не удалось получить временное хранилище для предпросмотра.");
            }

            var previewRepository = _repositoryCollectionService.CreateNewPhiladelphusRepository(previewStorage, needAutoName: false);
            previewRepository.Name = "Предпросмотр импорта";
            previewRepository.Description = "Временный репозиторий для предпросмотра дерева из Excel";
            return previewRepository;
        }

    }
}
