using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Platform.Storage;

using Philadelphus.Presentation.Avalonia.Helpers;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IFileDialogService" /> поверх <see cref="IStorageProvider" />.
    /// Storage API только асинхронный — синхронная сигнатура обеспечивается мостом <see cref="UiSync" />.
    /// Кандидат на рефакторинг интерфейса в async.
    /// </summary>
    public class AvaloniaFileDialogService : IFileDialogService
    {
        public string? OpenExcelFile()
            => PickOpen("Excel Files|*.xlsx;*.xls", title: "Выберите файл Excel");

        public string? SavePhjsonFile(string defaultFileName)
            => PickSave("PHJSON Files|*.phjson", suggestedName: defaultFileName);

        public string? OpenImportSchemaFile()
            => PickOpen("Import Schema|*.phimportschema.json|JSON|*.json", title: "Выберите шаблон схемы импорта");

        public string? SaveImportSchemaFile(string defaultFileName)
            => PickSave("Import Schema|*.phimportschema.json|JSON|*.json", suggestedName: defaultFileName);

        public string? BrowseLocalFile()
            => PickOpen(filter: null, title: null);

        public string? OpenFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
            => PickOpen(filter, title, initialDirectory);

        public string? SaveFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
            => PickSave(filter, suggestedName: null, defaultExtension, title, initialDirectory);

        public string? BrowseFolder(string? title = null, string? initialDirectory = null)
        {
            var storage = Storage;
            if (storage is null)
            {
                return null;
            }

            return UiSync.RunSync(async () =>
            {
                var options = new FolderPickerOpenOptions
                {
                    AllowMultiple = false,
                    Title = title,
                    SuggestedStartLocation = await ResolveStartLocation(storage, initialDirectory).ConfigureAwait(true)
                };

                var folders = await storage.OpenFolderPickerAsync(options).ConfigureAwait(true);
                return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
            });
        }

        private string? PickOpen(string? filter, string? title, string? initialDirectory = null)
        {
            var storage = Storage;
            if (storage is null)
            {
                return null;
            }

            return UiSync.RunSync(async () =>
            {
                var options = new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    Title = title,
                    FileTypeFilter = ParseFilter(filter),
                    SuggestedStartLocation = await ResolveStartLocation(storage, initialDirectory).ConfigureAwait(true)
                };

                var files = await storage.OpenFilePickerAsync(options).ConfigureAwait(true);
                return files.Count > 0 ? files[0].TryGetLocalPath() : null;
            });
        }

        private string? PickSave(string? filter, string? suggestedName, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
        {
            var storage = Storage;
            if (storage is null)
            {
                return null;
            }

            return UiSync.RunSync(async () =>
            {
                var options = new FilePickerSaveOptions
                {
                    Title = title,
                    SuggestedFileName = suggestedName,
                    DefaultExtension = defaultExtension,
                    FileTypeChoices = ParseFilter(filter),
                    SuggestedStartLocation = await ResolveStartLocation(storage, initialDirectory).ConfigureAwait(true)
                };

                var file = await storage.SaveFilePickerAsync(options).ConfigureAwait(true);
                return file?.TryGetLocalPath();
            });
        }

        private static async Task<IStorageFolder?> ResolveStartLocation(IStorageProvider storage, string? initialDirectory)
        {
            if (string.IsNullOrWhiteSpace(initialDirectory))
            {
                return null;
            }

            try
            {
                return await storage.TryGetFolderFromPathAsync(initialDirectory).ConfigureAwait(true);
            }
            catch
            {
                return null;
            }
        }

        private static List<FilePickerFileType>? ParseFilter(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return null;
            }

            var parts = filter.Split('|');
            var result = new List<FilePickerFileType>();

            for (var i = 0; i + 1 < parts.Length; i += 2)
            {
                var name = parts[i].Trim();
                var patterns = parts[i + 1]
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                result.Add(new FilePickerFileType(name) { Patterns = patterns });
            }

            return result.Count > 0 ? result : null;
        }

        private static IStorageProvider? Storage
        {
            get
            {
                var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                return owner?.StorageProvider;
            }
        }
    }
}
