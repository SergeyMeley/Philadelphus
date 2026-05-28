using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.ImportExport.Entities.DTOs;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Infrastructure.ImportExport.Phjson;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    /// <summary>
    /// Координирует проверку, предпросмотр и импорт данных из Excel.
    /// </summary>
    public class ExcelImportPipeline
    {
        private readonly ExcelImportExportAdapter _excelImportExportAdapter;
        private readonly IExcelImportProfileValidator _profileValidator;
        private readonly ExcelImportRepositoryPreviewBuilder _repositoryPreviewBuilder;
        private readonly JsonImportExportAdapter _jsonImportExportAdapter;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportPipeline" />.
        /// </summary>
        /// <param name="excelImportExportAdapter">Адаптер импорта данных из Excel.</param>
        /// <param name="profileValidator">Валидатор профилей импорта Excel.</param>
        /// <param name="repositoryPreviewBuilder">Построитель предпросмотра репозитория.</param>
        /// <param name="jsonImportExportAdapter">Адаптер импорта данных из PHJSON.</param>
        public ExcelImportPipeline(
            ExcelImportExportAdapter excelImportExportAdapter,
            IExcelImportProfileValidator profileValidator,
            ExcelImportRepositoryPreviewBuilder repositoryPreviewBuilder,
            JsonImportExportAdapter jsonImportExportAdapter)
        {
            _excelImportExportAdapter = excelImportExportAdapter;
            _profileValidator = profileValidator;
            _repositoryPreviewBuilder = repositoryPreviewBuilder;
            _jsonImportExportAdapter = jsonImportExportAdapter;
        }

        /// <summary>
        /// Возвращает профили Excel, включенные в выполнение импорта.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Коллекция профилей, включенных в импорт.</returns>
        public List<ExcelImportProfile> GetProfilesForExecution(ExcelImportSchema schema)
        {
            return ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
        }

        /// <summary>
        /// Проверяет схему импорта Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>Результат проверки схемы.</returns>
        public ExcelImportValidationResult Validate(ExcelImportSchema schema)
        {
            ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema);
            var profiles = GetProfilesForExecution(schema);
            return _profileValidator.ValidateProfiles(schema.SourceFilePath, profiles);
        }

        /// <summary>
        /// Формирует JSON-представление DTO рабочего дерева по схеме Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>JSON-представление DTO рабочего дерева.</returns>
        public string BuildJson(ExcelImportSchema schema)
        {
            return JsonSerializer.Serialize(BuildDto(schema), CreateJsonOptions());
        }

        /// <summary>
        /// Формирует DTO рабочего дерева по схеме Excel.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <returns>DTO рабочего дерева.</returns>
        public WorkingTreeExportDTO BuildDto(ExcelImportSchema schema)
        {
            EnsureValid(schema);
            return _excelImportExportAdapter.Parse(ExcelImportSchemaNormalizer.GetCanonicalExecutionSchema(schema));
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
            var json = BuildJson(schema);
            return _repositoryPreviewBuilder.Build(json, targetExistingRootName);
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
            var json = BuildJson(schema);
            _jsonImportExportAdapter.ImportFromJson(json, repositoryService, repository, _ => { }, (_, _) => { });
        }

        private void EnsureValid(ExcelImportSchema schema)
        {
            var validationResult = Validate(schema);
            if (validationResult.HasErrors)
                throw new InvalidOperationException(ExcelImportValidationMessageBuilder.Build(validationResult));
        }

        /// <summary>
        /// Проверяет, что в схеме есть хотя бы один включенный лист.
        /// </summary>
        /// <param name="schema">Схема импорта Excel.</param>
        /// <param name="emptySelectionMessage">Сообщение об ошибке для пустого набора листов.</param>
        public static void EnsureHasEnabledSheets(ExcelImportSchema schema, string emptySelectionMessage)
        {
            var profiles = ExcelImportSchemaNormalizer.GetEnabledProfiles(schema);
            if (profiles.Count == 0)
                throw new InvalidOperationException(emptySelectionMessage);
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}
