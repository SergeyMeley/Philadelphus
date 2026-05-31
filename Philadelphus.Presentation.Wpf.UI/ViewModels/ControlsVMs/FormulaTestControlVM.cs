using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
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
        /// Обозреватель текущего репозитория, из которого берется рабочий контекст формулы.
        /// </summary>
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;

        /// <summary>
        /// Доменный сервис, создающий недостающие системные листья результатов.
        /// </summary>
        private readonly IPhiladelphusRepositoryService _repositoryService;

        /// <summary>
        /// Текст формулы из поля ввода.
        /// </summary>
        private string _formulaText = "СУММ(2;3)";

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
            IPhiladelphusRepositoryService repositoryService,
            RepositoryExplorerControlVM repositoryExplorerControlVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _formulaEvaluator = formulaEvaluator ?? throw new ArgumentNullException(nameof(formulaEvaluator));
            _repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
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
        /// Команда вычисления текущей формулы.
        /// </summary>
        public RelayCommand EvaluateFormulaCommand => new RelayCommand(_ => EvaluateFormula());

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
                NotificationService = _notificationService
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

                _notificationService.SendTextMessage<FormulaTestControlVM>(
                    $"{errorCode}: {result.Error.Message}",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
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
