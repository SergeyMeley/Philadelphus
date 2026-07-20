using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.Factories.Implementations;

/// <summary>
/// Создаёт модели представления редактора коллекционного атрибута.
/// </summary>
public sealed class AttributeValuesCollectionVMFactory : IAttributeValuesCollectionVMFactory
{
    private readonly ILeaveAttributeValueService _attributeValueService;
    private readonly IRelayCommandFactory _commandFactory;
    private readonly IAttributeValueCreationConfirmationService _creationConfirmationService;
    private readonly List<WeakReference<AttributeValuesCollectionVM>> _openEditors = [];

    /// <summary>
    /// Инициализирует фабрику редактора коллекционного атрибута.
    /// </summary>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    /// <param name="commandFactory">Фабрика команды создания значения.</param>
    /// <param name="creationConfirmationService">Подтверждение добавления созданного значения.</param>
    public AttributeValuesCollectionVMFactory(
        ILeaveAttributeValueService attributeValueService,
        IRelayCommandFactory commandFactory,
        IAttributeValueCreationConfirmationService creationConfirmationService)
    {
        _attributeValueService = attributeValueService
            ?? throw new ArgumentNullException(nameof(attributeValueService));
        _commandFactory = commandFactory
            ?? throw new ArgumentNullException(nameof(commandFactory));
        _creationConfirmationService = creationConfirmationService
            ?? throw new ArgumentNullException(nameof(creationConfirmationService));
    }

    /// <inheritdoc />
    public AttributeValuesCollectionVM Create(ElementAttributeVM attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        var result = new AttributeValuesCollectionVM(
            attribute,
            _attributeValueService,
            _commandFactory,
            _creationConfirmationService);
        _openEditors.Add(new WeakReference<AttributeValuesCollectionVM>(result));
        return result;
    }

    /// <inheritdoc />
    public void RefreshOpenEditors() =>
        ForEachOpenEditor(editor => editor.Refresh());

    /// <inheritdoc />
    public void CloseOpenEditors()
    {
        ForEachOpenEditor(editor => editor.RequestClose());
        _openEditors.Clear();
    }

    private void ForEachOpenEditor(Action<AttributeValuesCollectionVM> action)
    {
        for (var index = _openEditors.Count - 1; index >= 0; index--)
        {
            if (_openEditors[index].TryGetTarget(out var editor) == false
                || editor.IsDisposed)
            {
                _openEditors.RemoveAt(index);
                continue;
            }

            action(editor);
        }
    }
}
