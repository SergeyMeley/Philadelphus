using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies
{
    /// <summary>
    /// Задает контракт для правил, подготавливающих значение перед записью.
    /// </summary>
    public interface IPrepareWriteValuePropertiesRule<T>
        where T : MainEntityBaseModel<T>
    {
        /// <summary>
        /// Подготавливает значение перед проверками <c>CanWrite</c> и фактической записью в поле модели.
        /// </summary>
        /// <remarks>
        /// Этот этап нужен для мягких преобразований, которые не должны блокировать присвоение:
        /// например, удаление запрещенных символов из <c>Name</c> или <c>CustomCode</c>.
        /// Важно, что все последующие правила получают уже подготовленное значение, поэтому проверка
        /// обязательности имени видит результат после удаления символов и <c>Trim()</c>.
        /// </remarks>
        /// <param name="model">Модель, свойство которой изменяется.</param>
        /// <param name="prop">Имя изменяемого свойства.</param>
        /// <param name="value">Исходное значение, переданное в setter.</param>
        /// <returns>Значение, которое будет передано в проверки записи и затем записано в модель.</returns>
        object PrepareWriteValue(T model, string prop, object value);
    }
}
