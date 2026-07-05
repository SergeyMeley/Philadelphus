namespace Philadelphus.Core.Domain.Identity.Configurations
{
    /// <summary>
    /// Конфигурация пользователя приложения.
    /// </summary>
    public class IdentityConfig
    {
        /// <summary>
        /// Имя пользователя, заданное вручную. Если не задано, используется имя текущей учетной записи.
        /// </summary>
        public string? UserName { get; set; }
    }
}
