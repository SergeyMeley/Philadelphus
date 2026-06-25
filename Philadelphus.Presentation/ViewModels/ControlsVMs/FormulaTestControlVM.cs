using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Editing;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Serilog;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления встроенного тестового интерфейса Formula Engine.
    /// </summary>
    public sealed class FormulaTestControlVM : ControlBaseVM, IFormulaEditorIntelliSenseVM
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
        /// Фабрика синхронных команд.
        /// </summary>
        private readonly IRelayCommandFactory _commandFactory;

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
        /// Признак отображения подсказки сигнатуры текущей функции.
        /// </summary>
        private bool _isFormulaSignatureHelpOpen;

        /// <summary>
        /// Текст сигнатуры текущей функции.
        /// </summary>
        private string _formulaSignatureText = string.Empty;

        /// <summary>
        /// Текст активного аргумента текущей функции.
        /// </summary>
        private string _formulaActiveArgumentText = string.Empty;

        /// <summary>
        /// Признак отображения визуальной подсветки текущей формулы.
        /// </summary>
        private bool _isFormulaHighlightOpen;

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
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        public FormulaTestControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IApplicationCommandsVM applicationCommandsVM,
            FormulaAstEvaluator formulaEvaluator,
            FormulaRegistry formulaRegistry,
            IPhiladelphusRepositoryService repositoryService,
            IFormulaDiagnosticsReporter formulaDiagnosticsReporter,
            RepositoryExplorerControlVM repositoryExplorerControlVM,
            IRelayCommandFactory commandFactory)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _formulaEvaluator = formulaEvaluator ?? throw new ArgumentNullException(nameof(formulaEvaluator));
            _formulaRegistry = formulaRegistry ?? throw new ArgumentNullException(nameof(formulaRegistry));
            _repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
            _formulaDiagnosticsReporter = formulaDiagnosticsReporter ?? throw new ArgumentNullException(nameof(formulaDiagnosticsReporter));
            _repositoryExplorerControlVM = repositoryExplorerControlVM ?? throw new ArgumentNullException(nameof(repositoryExplorerControlVM));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        /// <summary>
        /// Текст формулы из поля ввода.
        /// </summary>
        public string FormulaText
        {
            get => _formulaText;
            set
            {
                if (SetProperty(ref _formulaText, value))
                {
                    UpdateFormulaHighlights(0);
                }
            }
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
        /// Сегменты визуальной подсветки текущей формулы.
        /// </summary>
        public ObservableCollection<FormulaHighlightSegmentVM> FormulaHighlightSegments { get; } = new ObservableCollection<FormulaHighlightSegmentVM>();

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
        /// Признак отображения подсказки сигнатуры текущей функции.
        /// </summary>
        public bool IsFormulaSignatureHelpOpen
        {
            get => _isFormulaSignatureHelpOpen;
            private set => SetProperty(ref _isFormulaSignatureHelpOpen, value);
        }

        /// <summary>
        /// Текст сигнатуры текущей функции.
        /// </summary>
        public string FormulaSignatureText
        {
            get => _formulaSignatureText;
            private set => SetProperty(ref _formulaSignatureText, value);
        }

        /// <summary>
        /// Текст активного аргумента текущей функции.
        /// </summary>
        public string FormulaActiveArgumentText
        {
            get => _formulaActiveArgumentText;
            private set => SetProperty(ref _formulaActiveArgumentText, value);
        }

        /// <summary>
        /// Признак отображения визуальной подсветки текущей формулы.
        /// </summary>
        public bool IsFormulaHighlightOpen
        {
            get => _isFormulaHighlightOpen;
            private set => SetProperty(ref _isFormulaHighlightOpen, value);
        }

        /// <summary>
        /// Команда вычисления текущей формулы.
        /// </summary>
        public IRelayCommand EvaluateFormulaCommand => _commandFactory.Create(_ => EvaluateFormula());

        /// <summary>
        /// Обновляет предложения автодополнения по текущей позиции курсора.
        /// </summary>
        /// <param name="caretIndex">Позиция курсора в тексте формулы.</param>
        public void UpdateFormulaSuggestions(int caretIndex)
        {
            FormulaSuggestions.Clear();

            var source = FormulaText ?? string.Empty;
            UpdateFormulaHighlights(caretIndex);

            if (IsFormulaInput(source) == false)
            {
                IsFormulaSuggestionsOpen = false;
                SelectedFormulaSuggestion = null;
                CloseFormulaSignatureHelp();
                return;
            }

            UpdateFormulaSignatureHelp(source, caretIndex);

            if (TryGetCompletionPrefix(source, caretIndex, out var prefix, out _) == false)
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
        /// Ищет позицию парной скобки рядом с курсором.
        /// </summary>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <param name="matchingCaretIndex">Позиция курсора рядом с парной скобкой.</param>
        /// <returns>True, если парная скобка найдена.</returns>
        public bool TryGetMatchingParenthesisCaretIndex(int caretIndex, out int matchingCaretIndex)
        {
            matchingCaretIndex = caretIndex;

            var source = FormulaText ?? string.Empty;
            return FormulaEditorNavigation.TryGetMatchingParenthesisCaretIndex(
                source,
                caretIndex,
                out matchingCaretIndex);
        }

        /// <summary>
        /// Ищет диапазон текущего вызова функции для выделения в редакторе.
        /// </summary>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <param name="selectionStart">Начало выделения.</param>
        /// <param name="selectionLength">Длина выделения.</param>
        /// <returns>True, если текущий вызов функции найден.</returns>
        public bool TryGetCurrentFormulaCallSelection(
            int caretIndex,
            out int selectionStart,
            out int selectionLength)
        {
            selectionStart = 0;
            selectionLength = 0;

            var source = FormulaText ?? string.Empty;
            if (FormulaEditorNavigation.TryGetCurrentFormulaCallSelection(
                    source,
                    caretIndex,
                    out var selection) == false)
            {
                return false;
            }

            selectionStart = selection.Start;
            selectionLength = selection.Length;

            return selectionLength > 0;
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
        /// Обновляет цветные сегменты подсветки формулы.
        /// </summary>
        private void UpdateFormulaHighlights(int caretIndex)
        {
            FormulaHighlightSegments.Clear();

            var source = FormulaText ?? string.Empty;
            if (IsFormulaInput(source) == false)
            {
                IsFormulaHighlightOpen = false;
                return;
            }

            var tokenizerResult = FormulaTokenizer.Tokenize(source);
            var parserResult = FormulaParser.Parse(tokenizerResult.Tokens);
            var errorSpans = tokenizerResult.Errors
                .Concat(parserResult.Errors)
                .Select(error => error.Span)
                .Where(span => span is not null)
                .Select(span => NormalizeErrorSpan(source, span!.Value))
                .ToArray();
            var matchingParentheses = FindMatchingParentheses(tokenizerResult.Tokens, caretIndex);
            var currentPosition = 0;
            var tokens = tokenizerResult.Tokens.Where(token => token.Kind != FormulaTokenKind.End).ToArray();

            for (var tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
            {
                var token = tokens[tokenIndex];
                if (token.Span.Start > currentPosition)
                {
                    AddHighlightSegment(
                        source[currentPosition..token.Span.Start],
                        FormulaHighlightKind.Default,
                        currentPosition,
                        errorSpans,
                        matchingParentheses);
                }

                AddHighlightSegment(
                    token.Text,
                    GetHighlightKind(token, tokens, tokenIndex),
                    token.Span.Start,
                    errorSpans,
                    matchingParentheses);

                currentPosition = token.Span.Start + token.Span.Length;
            }

            if (currentPosition < source.Length)
            {
                AddHighlightSegment(
                    source[currentPosition..],
                    FormulaHighlightKind.Default,
                    currentPosition,
                    errorSpans,
                    matchingParentheses);
            }

            IsFormulaHighlightOpen = FormulaHighlightSegments.Count > 0;
        }

        /// <summary>
        /// Добавляет сегмент подсветки с учетом ошибок и парных скобок.
        /// </summary>
        private void AddHighlightSegment(
            string text,
            FormulaHighlightKind kind,
            int start,
            IReadOnlyCollection<FormulaTextSpan> errorSpans,
            IReadOnlySet<int> matchingParentheses)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var span = new FormulaTextSpan(start, text.Length);
            var hasError = errorSpans.Any(errorSpan => IsSpanOverlap(span, errorSpan));
            var isMatchingParenthesis = matchingParentheses.Contains(start);

            FormulaHighlightSegments.Add(new FormulaHighlightSegmentVM(
                text,
                hasError ? FormulaHighlightKind.Error : kind,
                isMatchingParenthesis));
        }

        /// <summary>
        /// Возвращает визуальный тип подсветки токена.
        /// </summary>
        private static FormulaHighlightKind GetHighlightKind(
            FormulaToken token,
            IReadOnlyList<FormulaToken> tokens,
            int tokenIndex)
        {
            return token.Kind switch
            {
                FormulaTokenKind.FormulaStart => FormulaHighlightKind.Operator,
                FormulaTokenKind.Identifier when IsFunctionIdentifier(tokens, tokenIndex) => FormulaHighlightKind.Function,
                FormulaTokenKind.Identifier => FormulaHighlightKind.Identifier,
                FormulaTokenKind.Number => FormulaHighlightKind.Number,
                FormulaTokenKind.String => FormulaHighlightKind.String,
                FormulaTokenKind.TreeLeaveReference => FormulaHighlightKind.TreeLeaveReference,
                FormulaTokenKind.OpenParenthesis or FormulaTokenKind.CloseParenthesis => FormulaHighlightKind.Parenthesis,
                FormulaTokenKind.Dot or FormulaTokenKind.Semicolon => FormulaHighlightKind.Punctuation,
                FormulaTokenKind.Plus
                    or FormulaTokenKind.Minus
                    or FormulaTokenKind.Star
                    or FormulaTokenKind.Slash
                    or FormulaTokenKind.Caret
                    or FormulaTokenKind.Ampersand
                    or FormulaTokenKind.Equal
                    or FormulaTokenKind.NotEqual
                    or FormulaTokenKind.Greater
                    or FormulaTokenKind.Less
                    or FormulaTokenKind.GreaterOrEqual
                    or FormulaTokenKind.LessOrEqual
                    or FormulaTokenKind.Question
                    or FormulaTokenKind.Colon => FormulaHighlightKind.Operator,
                _ => FormulaHighlightKind.Default
            };
        }

        /// <summary>
        /// Проверяет, является ли идентификатор именем вызова функции.
        /// </summary>
        private static bool IsFunctionIdentifier(IReadOnlyList<FormulaToken> tokens, int tokenIndex)
        {
            for (var index = tokenIndex + 1; index < tokens.Count; index++)
            {
                if (tokens[index].Kind == FormulaTokenKind.OpenParenthesis)
                {
                    return true;
                }

                if (tokens[index].Kind != FormulaTokenKind.End)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Находит пару скобок рядом с курсором.
        /// </summary>
        private static IReadOnlySet<int> FindMatchingParentheses(IReadOnlyList<FormulaToken> tokens, int caretIndex)
        {
            var parentheses = tokens
                .Where(token => token.Kind is FormulaTokenKind.OpenParenthesis or FormulaTokenKind.CloseParenthesis)
                .ToArray();
            var currentIndex = Array.FindIndex(
                parentheses,
                token => token.Span.Start == caretIndex || token.Span.Start + token.Span.Length == caretIndex);

            if (currentIndex < 0)
            {
                return new HashSet<int>();
            }

            var current = parentheses[currentIndex];
            var pairIndex = current.Kind == FormulaTokenKind.OpenParenthesis
                ? FindClosingParenthesis(parentheses, currentIndex)
                : FindOpeningParenthesis(parentheses, currentIndex);

            return pairIndex < 0
                ? new HashSet<int> { current.Span.Start }
                : new HashSet<int> { current.Span.Start, parentheses[pairIndex].Span.Start };
        }

        /// <summary>
        /// Находит закрывающую скобку для открывающей.
        /// </summary>
        private static int FindClosingParenthesis(IReadOnlyList<FormulaToken> parentheses, int openIndex)
        {
            var depth = 0;
            for (var index = openIndex; index < parentheses.Count; index++)
            {
                depth += parentheses[index].Kind == FormulaTokenKind.OpenParenthesis ? 1 : -1;
                if (depth == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Находит открывающую скобку для закрывающей.
        /// </summary>
        private static int FindOpeningParenthesis(IReadOnlyList<FormulaToken> parentheses, int closeIndex)
        {
            var depth = 0;
            for (var index = closeIndex; index >= 0; index--)
            {
                depth += parentheses[index].Kind == FormulaTokenKind.CloseParenthesis ? 1 : -1;
                if (depth == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Проверяет пересечение двух текстовых диапазонов.
        /// </summary>
        private static bool IsSpanOverlap(FormulaTextSpan first, FormulaTextSpan second)
        {
            var firstEnd = first.Start + first.Length;
            var secondEnd = second.Start + second.Length;
            return first.Start < secondEnd && second.Start < firstEnd;
        }

        /// <summary>
        /// Привязывает нулевой диапазон ошибки к видимому соседнему символу.
        /// </summary>
        private static FormulaTextSpan NormalizeErrorSpan(string source, FormulaTextSpan span)
        {
            if (span.Length > 0 || source.Length == 0)
            {
                return span;
            }

            if (span.Start > 0)
            {
                return new FormulaTextSpan(span.Start - 1, 1);
            }

            return new FormulaTextSpan(0, 1);
        }

        /// <summary>
        /// Обновляет подсказку сигнатуры по текущей позиции курсора.
        /// </summary>
        private void UpdateFormulaSignatureHelp(string source, int caretIndex)
        {
            if (TryGetActiveFormulaCall(source, caretIndex, out var activeCall) == false
                || _formulaRegistry.TryResolve(activeCall.Name, out var formula) == false
                || formula is null)
            {
                CloseFormulaSignatureHelp();
                return;
            }

            FormulaSignatureText = CreateSignatureText(formula);
            FormulaActiveArgumentText = CreateActiveArgumentText(formula, activeCall.ArgumentIndex);
            IsFormulaSignatureHelpOpen = true;
        }

        /// <summary>
        /// Закрывает подсказку сигнатуры.
        /// </summary>
        private void CloseFormulaSignatureHelp()
        {
            IsFormulaSignatureHelpOpen = false;
            FormulaSignatureText = string.Empty;
            FormulaActiveArgumentText = string.Empty;
        }

        /// <summary>
        /// Находит открытый вызов функции, внутри которого находится курсор.
        /// </summary>
        private static bool TryGetActiveFormulaCall(
            string source,
            int caretIndex,
            out ActiveFormulaCall activeCall)
        {
            activeCall = new ActiveFormulaCall(string.Empty, 0);
            if (caretIndex < 0 || caretIndex > source.Length)
            {
                return false;
            }

            var stack = new Stack<ActiveFormulaCall>();
            for (var index = 0; index < caretIndex; index++)
            {
                var current = source[index];
                if (current == '(')
                {
                    stack.Push(new ActiveFormulaCall(GetNameBeforeOpenParenthesis(source, index), 0));
                    continue;
                }

                if (current == ';' && stack.Count > 0)
                {
                    var top = stack.Pop();
                    stack.Push(top with { ArgumentIndex = top.ArgumentIndex + 1 });
                    continue;
                }

                if (current == ')' && stack.Count > 0)
                {
                    stack.Pop();
                }
            }

            var matchedCall = stack.FirstOrDefault(call => string.IsNullOrWhiteSpace(call.Name) == false);
            if (matchedCall is null)
            {
                return false;
            }

            activeCall = matchedCall;
            return true;
        }

        /// <summary>
        /// Получает имя функции перед открывающей скобкой.
        /// </summary>
        private static string GetNameBeforeOpenParenthesis(string source, int openParenthesisIndex)
        {
            var end = openParenthesisIndex - 1;
            while (end >= 0 && char.IsWhiteSpace(source[end]))
            {
                end--;
            }

            var start = end;
            while (start >= 0 && IsFormulaNameCharacter(source[start]))
            {
                start--;
            }

            return start == end
                ? string.Empty
                : source[(start + 1)..(end + 1)];
        }

        /// <summary>
        /// Создает текст сигнатуры формулы.
        /// </summary>
        private static string CreateSignatureText(FormulaDefinition formula)
        {
            var arguments = formula.Arguments.Count == 0
                ? string.Empty
                : string.Join("; ", formula.Arguments.Select(CreateArgumentSignatureText));

            return $"{formula.Name}({arguments})";
        }

        /// <summary>
        /// Создает текст активного аргумента.
        /// </summary>
        private static string CreateActiveArgumentText(FormulaDefinition formula, int argumentIndex)
        {
            if (formula.Arguments.Count == 0)
            {
                return "Аргументы не требуются";
            }

            var safeIndex = Math.Clamp(argumentIndex, 0, formula.Arguments.Count - 1);
            var argument = formula.Arguments[safeIndex];
            var optionalSuffix = argument.IsRequired ? string.Empty : " (необязательный)";
            var description = string.IsNullOrWhiteSpace(argument.Description)
                ? string.Empty
                : $" — {argument.Description}";

            return $"Аргумент {safeIndex + 1}: {CreateArgumentSignatureText(argument)}{optionalSuffix}{description}";
        }

        /// <summary>
        /// Создает фрагмент сигнатуры для аргумента.
        /// </summary>
        private static string CreateArgumentSignatureText(FormulaArgumentDefinition argument)
        {
            if (argument.IsRequired || argument.DefaultValue is null)
            {
                return argument.Name;
            }

            return $"{argument.Name} = {FormatDefaultValue(argument.DefaultValue)}";
        }

        /// <summary>
        /// Форматирует значение по умолчанию для подсказки.
        /// </summary>
        private static string FormatDefaultValue(object value)
        {
            return value switch
            {
                bool boolValue => boolValue ? "Истина" : "Ложь",
                string stringValue => $"\"{stringValue}\"",
                _ => value.ToString() ?? string.Empty
            };
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
}