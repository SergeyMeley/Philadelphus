using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Layout;
using global::Avalonia.Media;

using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Presentation.Avalonia.Views.Dialogs
{
    /// <summary>
    /// Диалог выбора хранилища данных.
    /// </summary>
    internal static class DataStorageSelectionDialog
    {
        public static Task<IDataStorageModel?> ShowAsync(
            Window? owner,
            IReadOnlyCollection<IDataStorageModel> dataStorages,
            string message,
            string title)
        {
            IDataStorageModel? result = null;
            var completionSource = new TaskCompletionSource<IDataStorageModel?>();

            var window = new Window
            {
                Title = title,
                Width = 440,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                WindowStartupLocation = owner == null
                    ? WindowStartupLocation.CenterScreen
                    : WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };

            var storageComboBox = new ComboBox
            {
                ItemsSource = dataStorages,
                PlaceholderText = "Выберите хранилище",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemTemplate = new FuncDataTemplate<IDataStorageModel>(
                    (storage, _) => new TextBlock { Text = storage.Name })
            };

            var selectButton = new Button
            {
                Content = "Выбрать",
                MinWidth = 90,
                IsDefault = true,
                IsEnabled = false
            };
            var cancelButton = new Button
            {
                Content = "Отмена",
                MinWidth = 90,
                IsCancel = true
            };

            storageComboBox.SelectionChanged += (_, _) =>
                selectButton.IsEnabled = storageComboBox.SelectedItem is IDataStorageModel;
            selectButton.Click += (_, _) =>
            {
                result = storageComboBox.SelectedItem as IDataStorageModel;
                window.Close();
            };
            cancelButton.Click += (_, _) => window.Close();

            window.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap
                    },
                    storageComboBox,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { selectButton, cancelButton }
                    }
                }
            };

            window.Closed += (_, _) => completionSource.TrySetResult(result);

            if (owner != null)
                _ = window.ShowDialog(owner);
            else
                window.Show();

            return completionSource.Task;
        }
    }
}
