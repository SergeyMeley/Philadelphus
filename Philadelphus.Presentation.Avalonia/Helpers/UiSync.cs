using global::Avalonia.Threading;

namespace Philadelphus.Presentation.Avalonia.Helpers
{
    /// <summary>
    /// Мост «синхронный вызов поверх асинхронного» для платформенных абстракций с синхронной
    /// сигнатурой (<c>IFileDialogService</c>, <c>IDialogService</c>), тогда как соответствующие
    /// Avalonia API (StorageProvider, Window.ShowDialog) только асинхронны.
    /// </summary>
    /// <remarks>
    /// На UI-потоке выполняется откачка очереди диспетчера до завершения задачи. Корректно
    /// работает для модальных окон/нативных диалогов. Кандидат на рефакторинг в async-интерфейсы.
    /// </remarks>
    internal static class UiSync
    {
        public static T RunSync<T>(Func<Task<T>> func)
        {
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                return Dispatcher.UIThread.InvokeAsync(() => RunSync(func)).GetTask().GetAwaiter().GetResult();
            }

            var task = func();
            while (task.IsCompleted == false)
            {
                Dispatcher.UIThread.RunJobs();
            }

            return task.GetAwaiter().GetResult();
        }

        public static void RunSync(Func<Task> func)
            => RunSync(async () =>
            {
                await func().ConfigureAwait(true);
                return true;
            });
    }
}
