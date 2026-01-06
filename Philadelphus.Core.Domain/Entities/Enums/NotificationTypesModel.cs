namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Тип уведомления
    /// </summary>
    public enum NotificationTypesModel
    {
        /// <summary>
        /// Нет типа
        /// </summary>
        None = 0,
        /// <summary>
        /// Текстовое сообщение
        /// </summary>
        TextMessage,
        /// <summary>
        /// Всплывающее попап-окно
        /// </summary>
        PopUpWindow,
        /// <summary>
        /// Модальное (блокирующее) окно
        /// </summary>
        ModalWindow,
        /// <summary>
        /// Электронное письмо
        /// </summary>
        Email,
        /// <summary>
        /// Смс-сообщение
        /// </summary>
        Sms,
        /// <summary>
        /// Телефонный звонок
        /// </summary>
        Call,
    }
}
