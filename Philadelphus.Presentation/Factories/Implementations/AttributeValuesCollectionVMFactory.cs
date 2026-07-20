using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.Factories.Implementations;

/// <summary>
/// Создаёт модели представления редактора коллекционного атрибута.
/// </summary>
public sealed class AttributeValuesCollectionVMFactory : IAttributeValuesCollectionVMFactory
{
    private readonly ILeaveAttributeValueService _attributeValueService;

    /// <summary>
    /// Инициализирует фабрику редактора коллекционного атрибута.
    /// </summary>
    /// <param name="attributeValueService">Сервис поиска значений листьев.</param>
    public AttributeValuesCollectionVMFactory(
        ILeaveAttributeValueService attributeValueService)
    {
        _attributeValueService = attributeValueService
            ?? throw new ArgumentNullException(nameof(attributeValueService));
    }

    /// <inheritdoc />
    public AttributeValuesCollectionVM Create(ElementAttributeVM attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        return new AttributeValuesCollectionVM(attribute, _attributeValueService);
    }
}
