using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;

using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Avalonia.Views.Dialogs;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация диалога выбора хранилища данных.
    /// </summary>
    public class AvaloniaDataStorageSelectionDialogService : IDataStorageSelectionDialogService
    {
        public Task<IDataStorageModel?> SelectAsync(
            IEnumerable<IDataStorageModel> dataStorages,
            string message,
            string title = "Выбор хранилища")
        {
            ArgumentNullException.ThrowIfNull(dataStorages);

            var storages = dataStorages.ToList();
            if (storages.Count == 0)
                return Task.FromResult<IDataStorageModel?>(null);

            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            return DataStorageSelectionDialog.ShowAsync(owner, storages, message, title);
        }
    }
}
