using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.Factories.Interfaces;

/// <summary>
/// Задаёт контракт создания редактора коллекционного атрибута.
/// </summary>
public interface IAttributeValuesCollectionVMFactory
{
    /// <summary>
    /// Создаёт редактор для зафиксированного атрибута.
    /// </summary>
    /// <param name="attribute">Модель представления коллекционного атрибута.</param>
    /// <returns>Созданная модель представления редактора.</returns>
    AttributeValuesCollectionVM Create(ElementAttributeVM attribute);
}
