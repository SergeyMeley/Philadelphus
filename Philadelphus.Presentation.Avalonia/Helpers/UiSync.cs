using global::Avalonia.Threading;

namespace Philadelphus.Presentation.Avalonia.Helpers
{
    /// <summary>
    /// Мост «синхронный вызов поверх асинхронного» для платформенных абстракций с синхронной
    /// сигнатурой (<c>IFileDialogService</c>, <c>IDialogService</c>), тогда как соответствующие
    /// Avalonia API (StorageProvider, Window.ShowDialog) только асинхронны.
    /// </summary>
    /// <remarks>
    /// На UI-потоке выполняется вложенный цикл диспетчера (<see cref="DispatcherFrame"/> +
    /// <see cref="Dispatcher.PushFrame"/>) до завершения задачи: он, в отличие от busy-loop с
    /// RunJobs(), корректно прокачивает ввод/рендер, поэтому нативные диалоги и модальные окна
    /// остаются интерактивными, а UI не зависает. Кандидат на рефакторинг в async-интерфейсы.
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
            if (task.IsCompleted == false)
            {
                var frame = new DispatcherFrame();
                // Continue — free-threaded (см. DispatcherFrame): можно завершать кадр из любого потока.
                task.ContinueWith(_ => frame.Continue = false, TaskScheduler.Default);
                Dispatcher.UIThread.PushFrame(frame);
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
