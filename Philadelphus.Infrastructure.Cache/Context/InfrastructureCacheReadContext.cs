namespace Philadelphus.Infrastructure.Cache.Context
{
    /// <summary>
    /// Контекст чтения кэшируемых данных инфраструктуры
    /// </summary>
    public sealed class InfrastructureCacheReadContext
    {
        /// <summary>
        /// Источник данных для уведомления
        /// </summary>
        public string SourceName
        {
            get
            {
                if (WasLoadedFromStorage && WasDistributedCacheUnavailable)
                {
                    return "БД (кэш временно недоступен)";
                }

                return WasLoadedFromStorage ? "БД" : "кэш";
            }
        }

        /// <summary>
        /// Признак чтения из хранилища
        /// </summary>
        private bool WasLoadedFromStorage { get; set; }

        /// <summary>
        /// Признак временной недоступности распределенного кэша
        /// </summary>
        private bool WasDistributedCacheUnavailable { get; set; }

        /// <summary>
        /// Отметить чтение из хранилища
        /// </summary>
        public void MarkStorageRead()
        {
            WasLoadedFromStorage = true;
        }

        /// <summary>
        /// Отметить временную недоступность распределенного кэша
        /// </summary>
        public void MarkDistributedCacheUnavailable()
        {
            WasDistributedCacheUnavailable = true;
        }
    }
}
