using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies
{
    /// <summary>
    /// Задает контракт для правил, подготавливающих значение перед записью.
    /// </summary>
    public interface IPrepareWriteValuePropertiesRule<T>
        where T : MainEntityBaseModel<T>
    {
        object PrepareWriteValue(T model, string prop, object value);
    }
}
