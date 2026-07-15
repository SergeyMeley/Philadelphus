namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    /// <summary>
    /// Коды ошибок привязки ссылочных значений атрибута.
    /// </summary>
    public static class AttributeReferenceErrorCodes
    {
        /// <summary>
        /// Тип данных атрибута не найден.
        /// </summary>
        public const string ValueTypeNotFound = "#ТИП_ДАННЫХ_НЕ_НАЙДЕН";

        /// <summary>
        /// Значение атрибута не найдено.
        /// </summary>
        public const string ValueNotFound = "#ЛИСТ_НЕ_НАЙДЕН";
    }
}
