using System.Threading.Tasks;

﻿namespace Philadelphus.Presentation.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? BrowseLocalFile();

        string? OpenExcelFile();

        string? SavePhjsonFile(string defaultFileName);

        string? OpenImportSchemaFile();

        string? SaveImportSchemaFile(string defaultFileName);

        string? OpenFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null);

        string? SaveFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null);

        string? BrowseFolder(string? title = null, string? initialDirectory = null);

        // Async-версии (дефолт — обёртка поверх синхронных; async-first платформы
        // переопределяют на нативную реализацию без блокировки UI-потока).
        Task<string?> BrowseLocalFileAsync() => Task.FromResult(BrowseLocalFile());

        Task<string?> OpenExcelFileAsync() => Task.FromResult(OpenExcelFile());

        Task<string?> SavePhjsonFileAsync(string defaultFileName) => Task.FromResult(SavePhjsonFile(defaultFileName));

        Task<string?> OpenImportSchemaFileAsync() => Task.FromResult(OpenImportSchemaFile());

        Task<string?> SaveImportSchemaFileAsync(string defaultFileName) => Task.FromResult(SaveImportSchemaFile(defaultFileName));

        Task<string?> OpenFileAsync(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
            => Task.FromResult(OpenFile(filter, defaultExtension, title, initialDirectory));

        Task<string?> SaveFileAsync(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
            => Task.FromResult(SaveFile(filter, defaultExtension, title, initialDirectory));

        Task<string?> BrowseFolderAsync(string? title = null, string? initialDirectory = null)
            => Task.FromResult(BrowseFolder(title, initialDirectory));
    }
}
