using global::Avalonia;
using global::Avalonia.Controls;
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

            var owner = ResolveVisibleOwner();
            return DataStorageSelectionDialog.ShowAsync(owner, storages, message, title);
        }

        /// <summary>
        /// Возвращает видимое окно-владельца, не используя скрытое стартовое окно приложения.
        /// </summary>
        private static Window? ResolveVisibleOwner()
        {
            var desktop = Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow is { IsVisible: true } mainWindow)
                return mainWindow;

            return desktop?.Windows.LastOrDefault(window => window.IsVisible);
        }
    }
}
