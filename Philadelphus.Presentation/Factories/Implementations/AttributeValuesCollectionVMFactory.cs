using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.Factories.Implementations;

/// <summary>
/// Создаёт модели представления редактора коллекционного атрибута.
/// </summary>
public sealed class AttributeValuesCollectionVMFactory : IAttributeValuesCollectionVMFactory
{
    /// <inheritdoc />
    public AttributeValuesCollectionVM Create(ElementAttributeVM attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        return new AttributeValuesCollectionVM(attribute);
    }
}
