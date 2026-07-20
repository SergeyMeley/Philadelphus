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

        return new AttributeValuesCollectionVM(
            attribute,
            _attributeValueService,
            _commandFactory,
            _creationConfirmationService);
    }
}
