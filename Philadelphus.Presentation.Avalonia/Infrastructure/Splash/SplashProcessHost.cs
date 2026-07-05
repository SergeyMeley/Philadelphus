using System.Diagnostics;
using System.IO;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Splash
{
    internal static class SplashProcessHost
    {
        public const string Argument = "--philadelphus-splash";

        private static string? _closeSignalPath;
        private static int _parentProcessId;

        public static string? StatusFilePath { get; private set; }

        public static bool ShouldClose
            => string.IsNullOrWhiteSpace(_closeSignalPath) == false && File.Exists(_closeSignalPath);

        public static bool TryInitialize(string[] args)
        {
            if (args.Length < 4 || args[0] != Argument)
            {
                return false;
            }

            StatusFilePath = args[1];
            _closeSignalPath = args[2];
            _ = int.TryParse(args[3], out _parentProcessId);
            return true;
        }

        public static bool IsParentAlive()
        {
            if (_parentProcessId <= 0)
            {
                return true;
            }

            try
            {
                var parentProcess = Process.GetProcessById(_parentProcessId);
                return parentProcess.HasExited == false;
            }
            catch
            {
                return false;
            }
        }

        public static void TryCleanup()
        {
            TryDeleteFile(StatusFilePath);
            TryDeleteFile(_closeSignalPath);

            var directoryPath = Path.GetDirectoryName(StatusFilePath);
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return;
            }

            try
            {
                if (Directory.Exists(directoryPath) && Directory.GetFiles(directoryPath).Length == 0)
                {
                    Directory.Delete(directoryPath);
                }
            }
            catch
            {
                // Временные файлы splash не критичны для работы приложения.
            }
        }

        private static void TryDeleteFile(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) == false && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Временные файлы splash не критичны для работы приложения.
            }
        }
    }
}
