namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Контейнер строки подключения
    /// </summary>
    public class ConnectionStringContainer
    {
        /// <summary>
        /// Уникальный идентификатор хранилища для сопоставления
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// Наименование провайдера БД
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        public string ConnectionString { get; set; }
        
    }
}
