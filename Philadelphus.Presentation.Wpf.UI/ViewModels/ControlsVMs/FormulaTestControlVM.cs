using AutoMapper;
using System.Collections.ObjectModel;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Serilog;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления встроенного тестового интерфейса Formula Engine.
    /// </summary>
    public sealed class FormulaTestControlVM : ControlBaseVM
    {
        /// <summary>
        /// Вычислитель формул, зарегистрированный в DI приложения.
        /// </summary>
        private readonly FormulaAstEvaluator _formulaEvaluator;

        /// <summary>
        /// Реестр формул, используемый для UX-подсказок редактора.
        /// </summary>
        private readonly FormulaRegistry _formulaRegistry;

        /// <summary>
        /// Обозреватель текущего репозитория, из которого берется рабочий контекст формулы.
        /// </summary>
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;

        /// <summary>
        /// Доменный сервис, создающий недостающие системные листья результатов.
        /// </summary>
        private readonly IPhiladelphusRepositoryService _repositoryService;

        /// <summary>
        /// Приемник диагностических сообщений Formula Engine.
        /// </summary>
        private readonly IFormulaDiagnosticsReporter _formulaDiagnosticsReporter;

        /// <summary>
        /// Текст формулы из поля ввода.
        /// </summary>
        private string _formulaText = "=СУММ(2;3)";

        /// <summary>
        /// Отображаемое значение результата.
        /// </summary>
        private string _resultText = string.Empty;

        /// <summary>
        /// Отображаемый тип результата.
        /// </summary>
        private string _resultTypeText = string.Empty;

        /// <summary>
        /// Отображаемая ошибка вычисления.
        /// </summary>
        private string _errorText = string.Empty;

        /// <summary>
        /// Признак открытия списка предложений редактора формул.
        /// </summary>
        private bool _isFormulaSuggestionsOpen;

        /// <summary>
        /// Выбранное предложение редактора формул.
        /// </summary>
        private FormulaSuggestionVM? _selectedFormulaSuggestion;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FormulaTestControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="formulaEvaluator">Вычислитель формул.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="repositoryExplorerControlVM">Обозреватель текущего репозитория.</param>
        public FormulaTestControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM,
            FormulaAstEvaluator formulaEvaluator,
            FormulaRegistry formulaRegistry,
            IPhiladelphusRepositoryService repositoryService,
            IFormulaDiagnosticsReporter formulaDiagnosticsReporter,
            RepositoryExplorerControlVM repositoryExplorerControlVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _formulaEvaluator = formulaEvaluator ?? throw new ArgumentNullException(nameof(formulaEvaluator));
            _formulaRegistry = formulaRegistry ?? throw new ArgumentNullException(nameof(formulaRegistry));
            _repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
            _formulaDiagnosticsReporter = formulaDiagnosticsReporter ?? throw new ArgumentNullException(nameof(formulaDiagnosticsReporter));
            _repositoryExplorerControlVM = repositoryExplorerControlVM ?? throw new ArgumentNullException(nameof(repositoryExplorerControlVM));
        }

        /// <summary>
        /// Текст формулы из поля ввода.
        /// </summary>
        public string FormulaText
        {
            get => _formulaText;
            set => SetProperty(ref _formulaText, value);
        }

        /// <summary>
        /// Отображаемое значение результата.
        /// </summary>
        public string ResultText
        {
            get => _resultText;
            private set => SetProperty(ref _resultText, value);
        }

        /// <summary>
        /// Отображаемый тип результата.
        /// </summary>
        public string ResultTypeText
        {
            get => _resultTypeText;
            private set => SetProperty(ref _resultTypeText, value);
        }

        /// <summary>
        /// Отображаемая ошибка вычисления.
        /// </summary>
        public string ErrorText
        {
            get => _errorText;
            private set => SetProperty(ref _errorText, value);
        }

        /// <summary>
        /// Предложения автодополнения формулы.
        /// </summary>
        public ObservableCollection<FormulaSuggestionVM> FormulaSuggestions { get; } = new ObservableCollection<FormulaSuggestionVM>();

        /// <summary>
        /// Признак открытия списка предложений редактора формул.
        /// </summary>
        public bool IsFormulaSuggestionsOpen
        {
            get => _isFormulaSuggestionsOpen;
            set => SetProperty(ref _isFormulaSuggestionsOpen, value);
        }

        /// <summary>
        /// Выбранное предложение редактора формул.
        /// </summary>
        public FormulaSuggestionVM? SelectedFormulaSuggestion
        {
            get => _selectedFormulaSuggestion;
            set => SetProperty(ref _selectedFormulaSuggestion, value);
        }

        /// <summary>
        /// Команда вычисления текущей формулы.
        /// </summary>
        public RelayCommand EvaluateFormulaCommand => new RelayCommand(_ => EvaluateFormula());

        /// <summary>
        /// Обновляет предложения автодополнения по текущей позиции курсора.
        /// </summary>
        /// <param name="caretIndex">Позиция курсора в тексте формулы.</param>
        public void UpdateFormulaSuggestions(int caretIndex)
        {
            FormulaSuggestions.Clear();

            var source = FormulaText ?? string.Empty;
            if (IsFormulaInput(source) == false || TryGetCompletionPrefix(source, caretIndex, out var prefix, out _) == false)
            {
                IsFormulaSuggestionsOpen = false;
                SelectedFormulaSuggestion = null;
                return;
            }

            var suggestions = _formulaRegistry.Formulas
                .SelectMany(CreateSuggestions)
                .Where(suggestion => IsSuggestionMatch(suggestion, prefix))
                .OrderBy(suggestion => suggestion.Category)
                .ThenBy(suggestion => suggestion.DisplayName)
                .Take(12)
                .ToList();

            foreach (var suggestion in suggestions)
            {
                FormulaSuggestions.Add(suggestion);
            }

            SelectedFormulaSuggestion = FormulaSuggestions.FirstOrDefault();
            IsFormulaSuggestionsOpen = FormulaSuggestions.Count > 0;
        }

        /// <summary>
        /// Перемещает выбор предложения вверх или вниз.
        /// </summary>
        /// <param name="offset">Смещение выбора.</param>
        public void MoveFormulaSuggestionSelection(int offset)
        {
            if (FormulaSuggestions.Count == 0)
            {
                return;
            }

            var currentIndex = SelectedFormulaSuggestion is null
                ? 0
                : FormulaSuggestions.IndexOf(SelectedFormulaSuggestion);

            var nextIndex = Math.Clamp(currentIndex + offset, 0, FormulaSuggestions.Count - 1);
            SelectedFormulaSuggestion = FormulaSuggestions[nextIndex];
        }

        /// <summary>
        /// Закрывает предложения автодополнения.
        /// </summary>
        public void CloseFormulaSuggestions()
        {
            IsFormulaSuggestionsOpen = false;
        }

        /// <summary>
        /// Подставляет выбранное предложение в текст формулы.
        /// </summary>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <returns>Новая позиция курсора после подстановки.</returns>
        public int ApplySelectedFormulaSuggestion(int caretIndex)
        {
            if (SelectedFormulaSuggestion is null
                || TryGetCompletionPrefix(FormulaText ?? string.Empty, caretIndex, out _, out var prefixStart) == false)
            {
                return caretIndex;
            }

            var source = FormulaText ?? string.Empty;
            var completion = SelectedFormulaSuggestion.CreateCompletion();
            FormulaText = source.Remove(prefixStart, caretIndex - prefixStart)
                .Insert(prefixStart, completion.Text);

            CloseFormulaSuggestions();
            return prefixStart + completion.CaretOffset;
        }

        /// <summary>
        /// Вычисляет текущую формулу и обновляет поля результата.
        /// </summary>
        private void EvaluateFormula()
        {
            try
            {
                var context = CreateExecutionContext();
                var result = _formulaEvaluator.Evaluate(FormulaText, context);
                ApplyResult(result);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Ошибка тестового вычисления формулы.");
                ResultText = string.Empty;
                ResultTypeText = string.Empty;
                ErrorText = exception.Message;
                _notificationService.SendTextMessage<FormulaTestControlVM>(
                    $"Ошибка тестового вычисления формулы. Подробности: {exception.Message}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
            }
        }

        /// <summary>
        /// Проверяет, что ввод является формулой.
        /// </summary>
        private static bool IsFormulaInput(string source)
        {
            return source.TrimStart().StartsWith("=", StringComparison.Ordinal);
        }

        /// <summary>
        /// Находит префикс имени функции перед курсором.
        /// </summary>
        private static bool TryGetCompletionPrefix(
            string source,
            int caretIndex,
            out string prefix,
            out int prefixStart)
        {
            prefix = string.Empty;
            prefixStart = caretIndex;

            if (caretIndex < 0 || caretIndex > source.Length)
            {
                return false;
            }

            var index = caretIndex - 1;
            while (index >= 0 && IsFormulaNameCharacter(source[index]))
            {
                index--;
            }

            prefixStart = index + 1;
            prefix = source[prefixStart..caretIndex];

            return caretIndex > 0 && (prefix.Length > 0 || source[Math.Max(0, caretIndex - 1)] == '=');
        }

        /// <summary>
        /// Проверяет символ имени формулы.
        /// </summary>
        private static bool IsFormulaNameCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        /// <summary>
        /// Создает предложения для имени формулы и ее псевдонимов.
        /// </summary>
        private static IEnumerable<FormulaSuggestionVM> CreateSuggestions(FormulaDefinition formula)
        {
            yield return new FormulaSuggestionVM(formula.Name, formula.Name, formula, useTemplate: true);

            foreach (var alias in formula.Aliases)
            {
                yield return new FormulaSuggestionVM(alias, alias, formula, useTemplate: false);
            }
        }

        /// <summary>
        /// Проверяет соответствие предложения введенному префиксу.
        /// </summary>
        private static bool IsSuggestionMatch(FormulaSuggestionVM suggestion, string prefix)
        {
            return string.IsNullOrWhiteSpace(prefix)
                || suggestion.DisplayName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Создает контекст вычисления формулы по текущему репозиторию.
        /// </summary>
        /// <returns>Контекст выполнения Formula Engine.</returns>
        private FormulaExecutionContext CreateExecutionContext()
        {
            var workingTree = ResolveWorkingTree();
            var systemBaseWorkingTree = _repositoryExplorerControlVM
                .PhiladelphusRepositoryVM
                .Model
                .ContentShrub
                .SystemBaseWorkingTree;

            return new FormulaExecutionContext
            {
                WorkingTree = workingTree,
                TreeLeaveResolver = workingTree is null ? null : new WorkingTreeTreeLeaveResolver(workingTree),
                SystemBaseWorkingTree = systemBaseWorkingTree,
                RepositoryService = _repositoryService,
                NotificationService = _notificationService,
                DiagnosticsReporter = _formulaDiagnosticsReporter
            };
        }

        /// <summary>
        /// Определяет рабочее дерево для формулы по выбранному элементу или первому пользовательскому дереву.
        /// </summary>
        /// <returns>Рабочее дерево или null, если репозиторий еще не содержит пользовательских деревьев.</returns>
        private WorkingTreeModel? ResolveWorkingTree()
        {
            if (_repositoryExplorerControlVM.SelectedRepositoryMember?.Model is IWorkingTreeMemberModel selectedMember)
            {
                return selectedMember.OwningWorkingTree;
            }

            var systemBaseWorkingTree = _repositoryExplorerControlVM
                .PhiladelphusRepositoryVM
                .Model
                .ContentShrub
                .SystemBaseWorkingTree;

            return _repositoryExplorerControlVM
                .PhiladelphusRepositoryVM
                .Model
                .ContentShrub
                .ContentWorkingTrees
                .FirstOrDefault(x => x.Uuid != systemBaseWorkingTree?.Uuid);
        }

        /// <summary>
        /// Переносит результат вычисления в отображаемые поля.
        /// </summary>
        /// <param name="result">Результат Formula Engine.</param>
        private void ApplyResult(FormulaResult result)
        {
            if (result.IsSuccess == false)
            {
                var errorCode = result.Error!.Code.GetDisplayName();
                ResultText = errorCode;
                ResultTypeText = string.Empty;
                ErrorText = result.Error.Message;

                return;
            }

            ResultText = FormatResultValue(result);
            ResultTypeText = result.ValueType.ToString();
            ErrorText = string.Empty;
        }

        /// <summary>
        /// Форматирует значение результата для вывода в тестовом интерфейсе.
        /// </summary>
        /// <param name="result">Результат Formula Engine.</param>
        /// <returns>Строковое представление результата.</returns>
        private static string FormatResultValue(FormulaResult result)
        {
            if (result.TreeLeave is not null)
            {
                return $"{result.TreeLeave.Name} [{result.TreeLeave.Uuid}]";
            }

            return result.Value?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Предложение автодополнения редактора формул.
    /// </summary>
    public sealed class FormulaSuggestionVM
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FormulaSuggestionVM" />.
        /// </summary>
        /// <param name="displayName">Отображаемое имя предложения.</param>
        /// <param name="insertName">Имя, подставляемое в формулу.</param>
        /// <param name="formula">Описание формулы.</param>
        /// <param name="useTemplate">Признак подстановки шаблона вызова функции.</param>
        public FormulaSuggestionVM(
            string displayName,
            string insertName,
            FormulaDefinition formula,
            bool useTemplate)
        {
            DisplayName = displayName;
            InsertName = insertName;
            Formula = formula;
            UseTemplate = useTemplate;
        }

        /// <summary>
        /// Отображаемое имя предложения.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Имя, подставляемое в формулу.
        /// </summary>
        public string InsertName { get; }

        /// <summary>
        /// Описание формулы.
        /// </summary>
        public FormulaDefinition Formula { get; }

        /// <summary>
        /// Признак подстановки шаблона вызова функции.
        /// </summary>
        public bool UseTemplate { get; }

        /// <summary>
        /// Категория формулы.
        /// </summary>
        public string Category => Formula.Category ?? "Формулы";

        /// <summary>
        /// Описание формулы.
        /// </summary>
        public string Description => Formula.Description ?? string.Empty;

        /// <summary>
        /// Сигнатура для отображения в списке предложений.
        /// </summary>
        public string Signature
        {
            get
            {
                var arguments = Formula.Arguments.Count == 0
                    ? string.Empty
                    : string.Join("; ", Formula.Arguments.Select(argument => argument.Name));

                return $"{Formula.Name}({arguments})";
            }
        }

        /// <summary>
        /// Создает текст подстановки и новую позицию курсора.
        /// </summary>
        /// <returns>Результат подстановки.</returns>
        public FormulaCompletionResult CreateCompletion()
        {
            if (UseTemplate == false)
            {
                return new FormulaCompletionResult(InsertName, InsertName.Length);
            }

            if (Formula.Arguments.Count <= 1)
            {
                return new FormulaCompletionResult($"{InsertName}()", InsertName.Length + 1);
            }

            var separators = string.Join(string.Empty, Enumerable.Repeat("; ", Formula.Arguments.Count - 1));
            return new FormulaCompletionResult($"{InsertName}({separators})", InsertName.Length + 1);
        }
    }

    /// <summary>
    /// Результат подстановки предложения автодополнения.
    /// </summary>
    /// <param name="Text">Текст подстановки.</param>
    /// <param name="CaretOffset">Позиция курсора относительно начала подстановки.</param>
    public sealed record FormulaCompletionResult(string Text, int CaretOffset);
}
