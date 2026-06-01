using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Управляет строкой формул и выбором ссылок на ячейки в обозревателе репозитория.
    /// </summary>
    public class RepositoryFormulaBarVM : ViewModelBase
    {
        private readonly RepositoryExplorerControlVM _repositoryExplorerVM;
        private readonly IPhiladelphusRepositoryService _service;
        private readonly FormulaAstEvaluator _formulaEvaluator;
        private readonly IFormulaDiagnosticsReporter _formulaDiagnosticsReporter;
        private readonly INotificationService _notificationService;
        private readonly ApplicationCommandsVM _applicationCommandsVM;

        private ElementAttributeVM? _selectedFormulaAttribute;
        private FormulaBarTarget? _formulaBarTarget;
        private string _formulaBarAddress = string.Empty;
        private string _formulaBarText = string.Empty;
        private string _formulaBarOriginalText = string.Empty;
        private int _formulaBarCaretIndex;
        private int _formulaBarSelectionStart;
        private int _formulaBarSelectionLength;
        private bool _isFormulaBarEditing;
        private bool _isFormulaBarEnabled;

        public RepositoryFormulaBarVM(
            RepositoryExplorerControlVM repositoryExplorerVM,
            IPhiladelphusRepositoryService service,
            FormulaAstEvaluator formulaEvaluator,
            IFormulaDiagnosticsReporter formulaDiagnosticsReporter,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(repositoryExplorerVM);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(formulaEvaluator);
            ArgumentNullException.ThrowIfNull(formulaDiagnosticsReporter);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(applicationCommandsVM);

            _repositoryExplorerVM = repositoryExplorerVM;
            _service = service;
            _formulaEvaluator = formulaEvaluator;
            _formulaDiagnosticsReporter = formulaDiagnosticsReporter;
            _notificationService = notificationService;
            _applicationCommandsVM = applicationCommandsVM;
        }

        /// <summary>
        /// Атрибут, выбранный для редактирования через строку формул во вкладке "Атрибуты".
        /// </summary>
        public ElementAttributeVM? SelectedFormulaAttribute
        {
            get => _selectedFormulaAttribute;
            set
            {
                if (SetProperty(ref _selectedFormulaAttribute, value))
                {
                    if (_repositoryExplorerVM.SelectedRepositoryMember != null)
                    {
                        _repositoryExplorerVM.SelectedRepositoryMember.SelectedAttributeVM = value;
                    }

                    SelectAttributeFormulaCell(value);
                }
            }
        }

        /// <summary>
        /// Адрес активной ячейки строки формул.
        /// </summary>
        public string FormulaBarAddress
        {
            get => _formulaBarAddress;
            private set => SetProperty(ref _formulaBarAddress, value);
        }

        /// <summary>
        /// Текст строки формул.
        /// </summary>
        public string FormulaBarText
        {
            get => _formulaBarText;
            set
            {
                if (SetProperty(ref _formulaBarText, value))
                {
                    OnPropertyChanged(nameof(IsFormulaReferenceSelectionActive));
                }
            }
        }

        /// <summary>
        /// Позиция каретки в строке формул.
        /// </summary>
        public int FormulaBarCaretIndex
        {
            get => _formulaBarCaretIndex;
            set => SetProperty(ref _formulaBarCaretIndex, value);
        }

        /// <summary>
        /// Начало выделенного фрагмента в строке формул.
        /// </summary>
        public int FormulaBarSelectionStart
        {
            get => _formulaBarSelectionStart;
            set => SetProperty(ref _formulaBarSelectionStart, value);
        }

        /// <summary>
        /// Длина выделенного фрагмента в строке формул.
        /// </summary>
        public int FormulaBarSelectionLength
        {
            get => _formulaBarSelectionLength;
            set => SetProperty(ref _formulaBarSelectionLength, value);
        }

        /// <summary>
        /// Признак активного редактирования строки формул.
        /// </summary>
        public bool IsFormulaBarEditing
        {
            get => _isFormulaBarEditing;
            set
            {
                if (SetProperty(ref _isFormulaBarEditing, value))
                {
                    OnPropertyChanged(nameof(IsFormulaReferenceSelectionActive));
                }
            }
        }

        /// <summary>
        /// Признак режима выбора ссылок на ячейки для текущей формулы.
        /// </summary>
        public bool IsFormulaReferenceSelectionActive => IsFormulaBarEditing
            && IsFormulaBarEnabled
            && (FormulaBarText ?? string.Empty).TrimStart().StartsWith("=", StringComparison.Ordinal);

        /// <summary>
        /// Признак доступности строки формул для выбранной ячейки.
        /// </summary>
        public bool IsFormulaBarEnabled
        {
            get => _isFormulaBarEnabled;
            private set
            {
                if (SetProperty(ref _isFormulaBarEnabled, value))
                {
                    OnPropertyChanged(nameof(IsFormulaReferenceSelectionActive));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Команда выбора атрибутной ячейки таблицы наследников для строки формул.
        /// </summary>
        public RelayCommand SelectChildFormulaCellCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (obj is ChildFormulaCellSelection selection)
                    {
                        SelectChildFormulaCell(selection);
                    }
                });
            }
        }

        /// <summary>
        /// Команда применения текста строки формул к активной ячейке.
        /// </summary>
        public RelayCommand ApplyFormulaBarCommand
        {
            get
            {
                return new RelayCommand(_ => ApplyFormulaBar(), _ => CanApplyFormulaBar());
            }
        }

        /// <summary>
        /// Команда отмены редактирования строки формул.
        /// </summary>
        public RelayCommand CancelFormulaBarCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    FormulaBarText = _formulaBarOriginalText;
                    FormulaBarCaretIndex = FormulaBarText.Length;
                    FormulaBarSelectionStart = FormulaBarCaretIndex;
                    FormulaBarSelectionLength = 0;
                    IsFormulaBarEditing = false;
                },
                _ => IsFormulaBarEnabled);
            }
        }

        /// <summary>
        /// Команда открытия отдельного редактора формул с текущим текстом строки формул.
        /// </summary>
        public RelayCommand OpenFormulaEditorFromFormulaBarCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    var request = new FormulaEditorOpenRequest(_repositoryExplorerVM, FormulaBarText);
                    if (_applicationCommandsVM.OpenFormulaEditorWindowCommand.CanExecute(request))
                    {
                        _applicationCommandsVM.OpenFormulaEditorWindowCommand.Execute(request);
                    }
                },
                _ => IsFormulaBarEnabled);
            }
        }

        private void SelectAttributeFormulaCell(ElementAttributeVM? attributeVM)
        {
            if (attributeVM == null)
            {
                ClearFormulaBarTarget();
                return;
            }

            var target = ResolveFormulaBarTarget(
                attributeVM.Model,
                $"{attributeVM.Name}",
                requireTargetInsideSelectedBranch: false);

            if (TryInsertFormulaReference(attributeVM.Model))
            {
                return;
            }

            SetFormulaBarTarget(target);
        }

        private void SelectChildFormulaCell(ChildFormulaCellSelection selection)
        {
            if (selection.IsAttribute == false
                || selection.SourceUuid == Guid.Empty
                || string.IsNullOrWhiteSpace(selection.ColumnKey))
            {
                ClearFormulaBarTarget();
                return;
            }

            var child = _repositoryExplorerVM.FindRepositoryMemberByUuid(selection.SourceUuid)?.Model as IAttributeOwnerModel;
            var attribute = child?.Attributes.FirstOrDefault(x =>
                string.Equals(x.Name, selection.ColumnKey, StringComparison.Ordinal));

            if (attribute == null)
            {
                ClearFormulaBarTarget();
                return;
            }

            var target = ResolveFormulaBarTarget(
                attribute,
                $"{attribute.Name} {selection.RowNumber}",
                requireTargetInsideSelectedBranch: true);

            if (TryInsertFormulaReference(attribute))
            {
                return;
            }

            SetFormulaBarTarget(target);
        }

        private FormulaBarTarget? ResolveFormulaBarTarget(
            ElementAttributeModel sourceAttribute,
            string address,
            bool requireTargetInsideSelectedBranch)
        {
            if (sourceAttribute.IsCollectionValue)
            {
                return CreateBlockedFormulaTarget(address, "Формулы для коллекционных значений атрибутов пока не поддерживаются.");
            }

            var targetAttribute = ResolveWritableAttribute(sourceAttribute);
            if (requireTargetInsideSelectedBranch
                && IsAttributeOwnerInsideSelectedBranch(targetAttribute.Owner) == false)
            {
                return CreateBlockedFormulaTarget(
                    address,
                    "Редактирование формулы ограничено узлом выше текущего контекста таблицы наследников.");
            }

            return new FormulaBarTarget(address, sourceAttribute, targetAttribute, Enabled: true, BlockReason: null);
        }

        private static ElementAttributeModel ResolveWritableAttribute(ElementAttributeModel attribute)
        {
            var current = attribute;
            while (current.IsOwn == false
                && current.InheritedAttributeFromParent?.Override == OverrideType.Sealed)
            {
                current = current.InheritedAttributeFromParent;
            }

            return current;
        }

        private static FormulaBarTarget CreateBlockedFormulaTarget(string address, string reason)
        {
            return new FormulaBarTarget(address, null, null, Enabled: false, BlockReason: reason);
        }

        private bool IsAttributeOwnerInsideSelectedBranch(IOwnerModel owner)
        {
            if (owner is not IMainEntityModel ownerEntity
                || _repositoryExplorerVM.SelectedRepositoryMember?.Model is not IMainEntityModel selectedEntity)
            {
                return false;
            }

            if (ownerEntity.Uuid == selectedEntity.Uuid)
            {
                return true;
            }

            if (owner is not IChildrenModel child)
            {
                return false;
            }

            var parent = child.Parent;
            while (parent is IMainEntityModel parentEntity)
            {
                if (parentEntity.Uuid == selectedEntity.Uuid)
                {
                    return true;
                }

                parent = parent is IChildrenModel parentChild
                    ? parentChild.Parent
                    : null;
            }

            return false;
        }

        private void SetFormulaBarTarget(FormulaBarTarget? target)
        {
            _formulaBarTarget = target;
            FormulaBarAddress = target?.Address ?? string.Empty;
            _formulaBarOriginalText = FormatAttributeValue(target?.SourceAttribute ?? target?.TargetAttribute);
            FormulaBarText = _formulaBarOriginalText;
            FormulaBarCaretIndex = FormulaBarText.Length;
            FormulaBarSelectionStart = FormulaBarCaretIndex;
            FormulaBarSelectionLength = 0;
            IsFormulaBarEditing = false;
            IsFormulaBarEnabled = target?.Enabled == true;

            if (target is { Enabled: false }
                && string.IsNullOrWhiteSpace(target.BlockReason) == false)
            {
                _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                    target.BlockReason,
                    NotificationCriticalLevelModel.Warning);
            }
        }

        private void ClearFormulaBarTarget()
        {
            _formulaBarTarget = null;
            FormulaBarAddress = string.Empty;
            _formulaBarOriginalText = string.Empty;
            FormulaBarText = string.Empty;
            FormulaBarCaretIndex = 0;
            FormulaBarSelectionStart = 0;
            FormulaBarSelectionLength = 0;
            IsFormulaBarEditing = false;
            IsFormulaBarEnabled = false;
        }

        private bool CanApplyFormulaBar()
        {
            return IsFormulaBarEnabled
                && _repositoryExplorerVM.IsRepositoryLoading == false
                && _formulaBarTarget?.TargetAttribute != null;
        }

        private void ApplyFormulaBar()
        {
            if (_formulaBarTarget?.TargetAttribute == null)
            {
                return;
            }

            var targetAttribute = _formulaBarTarget.TargetAttribute;
            if (TryApplyFormulaBarText(targetAttribute, FormulaBarText) == false)
            {
                return;
            }

            _formulaBarOriginalText = FormatAttributeValue(targetAttribute);
            FormulaBarText = _formulaBarOriginalText;
            FormulaBarCaretIndex = FormulaBarText.Length;
            FormulaBarSelectionStart = FormulaBarCaretIndex;
            FormulaBarSelectionLength = 0;
            IsFormulaBarEditing = false;
            NotifyFormulaAttributeChanged(targetAttribute);
            RecalculateDependentFormulas(targetAttribute);
        }

        private bool TryApplyFormulaBarText(ElementAttributeModel targetAttribute, string text)
        {
            if (string.IsNullOrWhiteSpace(text) == false
                && text.TrimStart().StartsWith("=", StringComparison.Ordinal))
            {
                var result = _formulaEvaluator.Evaluate(text, CreateFormulaExecutionContext(targetAttribute));
                if (result.IsSuccess == false)
                {
                    targetAttribute.ValueFormula = text.Trim();
                    targetAttribute.ValueFormulaErrorCode = FormatFormulaErrorCode(result);
                    _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                        $"Формула сохранена, но не вычислена: {result.Error?.Message}",
                        NotificationCriticalLevelModel.Warning);
                    return true;
                }

                return TryApplyFormulaResult(targetAttribute, result, text.Trim());
            }

            return TryApplyPlainFormulaBarText(targetAttribute, text);
        }

        private bool TryApplyFormulaResult(ElementAttributeModel targetAttribute, FormulaResult result, string formulaText)
        {
            if (result.TreeLeave != null)
            {
                if (IsTreeLeaveCompatible(targetAttribute, result.TreeLeave) == false)
                {
                    SendFormulaTypeMismatch(targetAttribute, result.TreeLeave.ParentNode.Name);
                    return false;
                }

                targetAttribute.Value = result.TreeLeave;
                targetAttribute.ValueFormula = formulaText;
                targetAttribute.ValueFormulaErrorCode = string.Empty;
                return true;
            }

            if (targetAttribute.ValueType is not SystemBaseTreeNodeModel systemBaseNode)
            {
                SendFormulaTypeMismatch(targetAttribute, result.ValueType.ToString());
                return false;
            }

            if (IsSystemBaseResultCompatible(systemBaseNode.SystemBaseType, result.ValueType) == false
                || SystemBaseStringValueValidator.TryFormat(systemBaseNode.SystemBaseType, result.Value, out var stringValue) == false)
            {
                SendFormulaTypeMismatch(targetAttribute, result.ValueType.ToString());
                return false;
            }

            if (targetAttribute.TrySetSystemBaseValueFromString(stringValue) == false)
            {
                return false;
            }

            targetAttribute.ValueFormula = formulaText;
            targetAttribute.ValueFormulaErrorCode = string.Empty;
            return true;
        }

        private bool TryApplyPlainFormulaBarText(ElementAttributeModel targetAttribute, string text)
        {
            var trimmedText = text.Trim();
            if (TryGetLeafUuidReference(trimmedText, out var valueUuid)
                && targetAttribute.ValuesList?.FirstOrDefault(x => x.Uuid == valueUuid) is TreeLeaveModel referencedValue)
            {
                targetAttribute.Value = referencedValue;
                targetAttribute.ValueFormula = string.Empty;
                targetAttribute.ValueFormulaErrorCode = string.Empty;
                return true;
            }

            if (targetAttribute.ValueType is SystemBaseTreeNodeModel)
            {
                if (targetAttribute.TrySetSystemBaseValueFromString(text) == false)
                {
                    return false;
                }

                targetAttribute.ValueFormula = string.Empty;
                targetAttribute.ValueFormulaErrorCode = string.Empty;
                return true;
            }

            var value = targetAttribute.ValuesList?.FirstOrDefault(x =>
                string.Equals(x.Name, text, StringComparison.Ordinal));
            if (value == null)
            {
                _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                    $"Значение '{text}' не найдено среди допустимых значений атрибута '{targetAttribute.Name}'.",
                    NotificationCriticalLevelModel.Warning);
                return false;
            }

            targetAttribute.Value = value;
            targetAttribute.ValueFormula = string.Empty;
            targetAttribute.ValueFormulaErrorCode = string.Empty;
            return true;
        }

        private FormulaExecutionContext CreateFormulaExecutionContext(ElementAttributeModel? targetAttribute = null)
        {
            var workingTree = ResolveWorkingTree();
            var systemBaseWorkingTree = _repositoryExplorerVM.PhiladelphusRepositoryVM.Model.ContentShrub.SystemBaseWorkingTree;

            return new FormulaExecutionContext
            {
                WorkingTree = workingTree,
                TreeLeaveResolver = workingTree is null ? null : new WorkingTreeTreeLeaveResolver(workingTree),
                SystemBaseWorkingTree = systemBaseWorkingTree,
                CurrentAttributeOwner = targetAttribute?.Owner as IAttributeOwnerModel,
                RepositoryService = _service,
                NotificationService = _notificationService,
                DiagnosticsReporter = _formulaDiagnosticsReporter
            };
        }

        private WorkingTreeModel? ResolveWorkingTree()
        {
            if (_repositoryExplorerVM.SelectedRepositoryMember?.Model is IWorkingTreeMemberModel selectedMember)
            {
                return selectedMember.OwningWorkingTree;
            }

            var systemBaseWorkingTree = _repositoryExplorerVM.PhiladelphusRepositoryVM.Model.ContentShrub.SystemBaseWorkingTree;
            return _repositoryExplorerVM.PhiladelphusRepositoryVM.Model.ContentShrub.ContentWorkingTrees
                .FirstOrDefault(x => x.Uuid != systemBaseWorkingTree?.Uuid);
        }

        private static bool IsTreeLeaveCompatible(ElementAttributeModel attribute, TreeLeaveModel value)
        {
            return attribute.ValueType?.Uuid == value.ParentNode.Uuid;
        }

        private static bool IsSystemBaseResultCompatible(SystemBaseType expectedType, SystemBaseType actualType)
        {
            return expectedType == actualType
                || expectedType == SystemBaseType.OBJECT
                || expectedType == SystemBaseType.NUMERIC && actualType is SystemBaseType.INTEGER or SystemBaseType.FLOAT;
        }

        private void SendFormulaTypeMismatch(ElementAttributeModel attribute, string actualType)
        {
            _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                $"Тип результата формулы '{actualType}' не соответствует типу данных атрибута '{attribute.Name}' ({attribute.ValueType?.Name}).",
                NotificationCriticalLevelModel.Warning);
        }

        private void NotifyFormulaAttributeChanged(ElementAttributeModel attribute)
        {
            _repositoryExplorerVM.NotifyFormulaPropertyListChanged();
            _repositoryExplorerVM.RebuildChildCollectionTable();

            if (_selectedFormulaAttribute?.Model.Uuid == attribute.Uuid)
            {
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.AssignedValue));
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.AssignedValueText));
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.DisplayedValueText));
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.FormulaValueText));
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.IsValueOverridden));
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.ValueOverrideToolTip));
                _selectedFormulaAttribute.OnPropertyChanged(nameof(ElementAttributeVM.State));
            }

            if (attribute.Owner is IMainEntityModel owner)
            {
                if (_repositoryExplorerVM.FindRepositoryMemberByUuid(owner.Uuid) is ViewModelBase ownerVM)
                {
                    ownerVM.OnPropertyChanged(nameof(IMainEntityVM<IMainEntityModel>.AttributesVMs));
                }
            }

            _repositoryExplorerVM.NotifyRepositoryStateChanged();
        }

        private void RecalculateDependentFormulas(ElementAttributeModel changedAttribute)
        {
            RecalculateDependentFormulas(changedAttribute, new HashSet<Guid> { changedAttribute.Uuid });
        }

        private void RecalculateDependentFormulas(ElementAttributeModel changedAttribute, ISet<Guid> visited)
        {
            if (changedAttribute.Owner is not IAttributeOwnerModel owner)
            {
                return;
            }

            var dependents = owner.Attributes
                .Where(x => x.Uuid != changedAttribute.Uuid
                    && visited.Contains(x.Uuid) == false
                    && string.IsNullOrWhiteSpace(x.ValueFormula) == false
                    && FormulaReferencesAttribute(x.ValueFormula, changedAttribute))
                .ToList();

            foreach (var dependent in dependents)
            {
                visited.Add(dependent.Uuid);

                var result = _formulaEvaluator.Evaluate(
                    dependent.ValueFormula,
                    CreateFormulaExecutionContext(dependent));

                if (result.IsSuccess == false)
                {
                    dependent.ValueFormulaErrorCode = FormatFormulaErrorCode(result);
                    NotifyFormulaAttributeChanged(dependent);
                    continue;
                }

                if (TryApplyFormulaResult(dependent, result, dependent.ValueFormula))
                {
                    NotifyFormulaAttributeChanged(dependent);
                    RecalculateDependentFormulas(dependent, visited);
                }
            }
        }

        private bool TryInsertFormulaReference(ElementAttributeModel referencedAttribute)
        {
            if (IsFormulaBarEditing == false
                || _formulaBarTarget?.TargetAttribute == null
                || IsFormulaBarEnabled == false)
            {
                return false;
            }

            if (_formulaBarTarget.TargetAttribute.Uuid == referencedAttribute.Uuid)
            {
                return true;
            }

            if (CanUseRelativeAttributeReference(_formulaBarTarget.TargetAttribute, referencedAttribute) == false)
            {
                _notificationService.SendTextMessage<RepositoryExplorerControlVM>(
                    "Относительная ссылка АТРИБУТ доступна только для атрибутов одного и того же элемента.",
                    NotificationCriticalLevelModel.Warning);
                return true;
            }

            var reference = CreateRelativeAttributeReference(referencedAttribute);
            var text = FormulaBarText ?? string.Empty;
            if (text.TrimStart().StartsWith("=", StringComparison.Ordinal) == false)
            {
                return false;
            }

            var caretIndex = Math.Clamp(FormulaBarCaretIndex, 0, text.Length);
            var replacementStart = Math.Clamp(FormulaBarSelectionStart, 0, text.Length);
            var replacementLength = Math.Clamp(FormulaBarSelectionLength, 0, text.Length - replacementStart);

            if (replacementLength == 0
                && TryFindAttributeReferenceAtCaret(text, caretIndex, out var referenceStart, out var referenceLength))
            {
                replacementStart = referenceStart;
                replacementLength = referenceLength;
            }
            else if (replacementLength == 0
                && TryFindOperandAtCaret(text, caretIndex, out var operandStart, out var operandLength))
            {
                replacementStart = operandStart;
                replacementLength = operandLength;
            }

            FormulaBarText = text.Remove(replacementStart, replacementLength).Insert(replacementStart, reference);
            FormulaBarCaretIndex = replacementStart + reference.Length;
            FormulaBarSelectionStart = FormulaBarCaretIndex;
            FormulaBarSelectionLength = 0;
            return true;
        }

        private static bool CanUseRelativeAttributeReference(
            ElementAttributeModel targetAttribute,
            ElementAttributeModel referencedAttribute)
        {
            return targetAttribute.Owner?.Uuid == referencedAttribute.Owner?.Uuid;
        }

        private static string CreateRelativeAttributeReference(ElementAttributeModel attribute)
        {
            var escapedName = (attribute.Name ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"АТРИБУТ(\"{escapedName}\")";
        }

        private static bool TryFindAttributeReferenceAtCaret(
            string text,
            int caretIndex,
            out int referenceStart,
            out int referenceLength)
        {
            referenceStart = 0;
            referenceLength = 0;

            const string functionName = "АТРИБУТ";
            var searchIndex = 0;

            while (searchIndex < text.Length)
            {
                var nameIndex = text.IndexOf(functionName, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (nameIndex < 0)
                {
                    return false;
                }

                var openParenthesisIndex = SkipWhiteSpace(text, nameIndex + functionName.Length);
                if (openParenthesisIndex >= text.Length
                    || text[openParenthesisIndex] != '('
                    || TryFindClosingParenthesis(text, openParenthesisIndex, out var closeParenthesisIndex) == false)
                {
                    searchIndex = nameIndex + functionName.Length;
                    continue;
                }

                var endIndex = closeParenthesisIndex + 1;
                if (caretIndex >= nameIndex && caretIndex <= endIndex)
                {
                    referenceStart = nameIndex;
                    referenceLength = endIndex - nameIndex;
                    return true;
                }

                searchIndex = endIndex;
            }

            return false;
        }

        private static bool TryFindOperandAtCaret(
            string text,
            int caretIndex,
            out int operandStart,
            out int operandLength)
        {
            operandStart = 0;
            operandLength = 0;

            if (text.Length == 0)
            {
                return false;
            }

            var tokenStart = Math.Clamp(caretIndex, 0, text.Length);
            var tokenEnd = tokenStart;

            while (tokenStart > 0 && char.IsWhiteSpace(text[tokenStart - 1]))
            {
                tokenStart--;
            }

            if (tokenStart > 0 && text[tokenStart - 1] == '"')
            {
                return TryFindStringLiteralBeforeCaret(text, tokenStart, out operandStart, out operandLength);
            }

            while (tokenStart > 0 && IsOperandBoundary(text[tokenStart - 1]) == false)
            {
                tokenStart--;
            }

            while (tokenEnd < text.Length && char.IsWhiteSpace(text[tokenEnd]))
            {
                tokenEnd++;
            }

            while (tokenEnd < text.Length && IsOperandBoundary(text[tokenEnd]) == false)
            {
                tokenEnd++;
            }

            if (tokenEnd <= tokenStart)
            {
                return false;
            }

            var token = text[tokenStart..tokenEnd].Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            operandStart = tokenStart;
            operandLength = tokenEnd - tokenStart;
            return true;
        }

        private static bool TryFindStringLiteralBeforeCaret(
            string text,
            int caretIndex,
            out int operandStart,
            out int operandLength)
        {
            operandStart = 0;
            operandLength = 0;

            var start = caretIndex - 1;
            while (start > 0)
            {
                start--;
                if (text[start] != '"')
                {
                    continue;
                }

                var escapedQuotes = 0;
                for (var i = start + 1; i < caretIndex - 1 && text[i] == '"'; i++)
                {
                    escapedQuotes++;
                }

                if (escapedQuotes % 2 == 0)
                {
                    operandStart = start;
                    operandLength = caretIndex - start;
                    return true;
                }
            }

            return false;
        }

        private static bool IsOperandBoundary(char value)
        {
            return char.IsWhiteSpace(value)
                || value is ';' or ',' or '+' or '-' or '*' or '/' or '(' or ')' or '>' or '<' or '=' or '!';
        }

        private static int SkipWhiteSpace(string text, int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            return index;
        }

        private static bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex)
        {
            closeParenthesisIndex = -1;
            var depth = 0;
            var isInString = false;

            for (var i = openParenthesisIndex; i < text.Length; i++)
            {
                var current = text[i];
                if (current == '"')
                {
                    if (isInString && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    isInString = !isInString;
                    continue;
                }

                if (isInString)
                {
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current != ')')
                {
                    continue;
                }

                depth--;
                if (depth == 0)
                {
                    closeParenthesisIndex = i;
                    return true;
                }
            }

            return false;
        }

        private static bool FormulaReferencesAttribute(string formula, ElementAttributeModel attribute)
        {
            return formula.Contains(CreateRelativeAttributeReference(attribute), StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatAttributeValue(ElementAttributeModel? attribute)
        {
            if (attribute == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(attribute.ValueFormula) == false)
            {
                return attribute.ValueFormula;
            }

            if (attribute.IsCollectionValue)
            {
                return string.Join("; ", attribute.Values.Select(x => x.Name));
            }

            return attribute.Value?.Uuid == null
                ? string.Empty
                : $"[{attribute.Value.Uuid}]";
        }

        private static bool TryGetLeafUuidReference(string text, out Guid uuid)
        {
            uuid = Guid.Empty;

            return text.Length == 38
                && text.StartsWith("[", StringComparison.Ordinal)
                && text.EndsWith("]", StringComparison.Ordinal)
                && Guid.TryParse(text[1..^1], out uuid);
        }

        private static string FormatFormulaErrorCode(FormulaResult result)
        {
            return result.Error == null
                ? "#ERROR!"
                : $"#{result.Error.Code}!";
        }

        private sealed record FormulaBarTarget(
            string Address,
            ElementAttributeModel? SourceAttribute,
            ElementAttributeModel? TargetAttribute,
            bool Enabled,
            string? BlockReason);
    }

    /// <summary>
    /// Описывает выбранную атрибутную ячейку плоской таблицы наследников.
    /// </summary>
    /// <param name="SourceUuid">Uuid элемента строки.</param>
    /// <param name="ColumnKey">Логический ключ колонки.</param>
    /// <param name="RowNumber">Порядковый номер строки в таблице.</param>
    /// <param name="IsAttribute">Признак атрибутной колонки.</param>
    public sealed record ChildFormulaCellSelection(
        Guid SourceUuid,
        string ColumnKey,
        int RowNumber,
        bool IsAttribute);
}
