using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies
{
    /// <summary>
    /// Задает контракт для правил чтения и записи значений свойств.
    /// </summary>
    public interface IPropertiesRule<T>
        where T : MainEntityBaseModel<T>
    {
        bool CanRead(T model, string prop);

        bool CanWrite(T model, string prop, object value);

        object OnRead(T model, string prop, object value);

        void OnWrite(T model, string prop, object oldValue, object newValue);
    }
}
