namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Контракт отображения прогресса длительной операции импорта.
    /// Реализация инкапсулирует платформенное окно и маршалинг вызовов на UI-поток.
    /// </summary>
    public interface IImportProgressReporter
    {
        /// <summary>
        /// Открывает индикатор прогресса с заголовком и начальным статусом.
        /// </summary>
        void Begin(string header, string status);

        /// <summary>
        /// Обновляет текущий статус операции.
        /// </summary>
        void Report(string status);

        /// <summary>
        /// Переводит индикатор в состояние успешного завершения.
        /// </summary>
        void Complete(string status);

        /// <summary>
        /// Переводит индикатор в состояние ошибки.
        /// </summary>
        void Fail(string status);
    }
}
