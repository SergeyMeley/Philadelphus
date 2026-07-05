using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Splash
{
    internal sealed class ExternalSplashController : ISplashController
    {
        private readonly object _syncRoot = new();
        private readonly Process _process;
        private readonly string _closeSignalPath;
        private readonly string _statusFilePath;

        private ExternalSplashController(Process process, string statusFilePath, string closeSignalPath)
        {
            _process = process;
            _statusFilePath = statusFilePath;
            _closeSignalPath = closeSignalPath;
        }

        public static ExternalSplashController? TryStart()
        {
            try
            {
                var processPath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(processPath))
                {
                    return null;
                }

                var directoryPath = Path.Combine(
                    Path.GetTempPath(),
                    "Philadelphus.Splash",
                    Guid.NewGuid().ToString("N"));

                Directory.CreateDirectory(directoryPath);
                var statusFilePath = Path.Combine(directoryPath, "status.txt");
                var closeSignalPath = Path.Combine(directoryPath, "close.signal");
                File.WriteAllText(statusFilePath, "Подготовка запуска...");

                var startInfo = new ProcessStartInfo(processPath)
                {
                    UseShellExecute = false,
                };
                startInfo.ArgumentList.Add(SplashProcessHost.Argument);
                startInfo.ArgumentList.Add(statusFilePath);
                startInfo.ArgumentList.Add(closeSignalPath);
                startInfo.ArgumentList.Add(Environment.ProcessId.ToString(CultureInfo.InvariantCulture));

                var process = Process.Start(startInfo);
                return process == null
                    ? null
                    : new ExternalSplashController(process, statusFilePath, closeSignalPath);
            }
            catch
            {
                return null;
            }
        }

        public void SetStatus(string status)
        {
            lock (_syncRoot)
            {
                TryWriteText(_statusFilePath, string.IsNullOrWhiteSpace(status) ? "Загрузка..." : status);
            }
        }

        public void Close()
        {
            lock (_syncRoot)
            {
                TryWriteText(_closeSignalPath, string.Empty);
            }

            try
            {
                if (_process.HasExited == false)
                {
                    _process.WaitForExit(1000);
                }
            }
            catch
            {
                // Закрытие splash не должно ломать завершение запуска основного приложения.
            }
        }

        private static void TryWriteText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch
            {
                // Статус splash информативный; сбой записи не критичен для запуска.
            }
        }
    }
}
