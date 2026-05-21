using Microsoft.Win32;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public interface IFileDialogService
    {
        string? OpenExcelFile();

        string? SavePhjsonFile(string defaultFileName);

        string? OpenImportSchemaFile();

        string? SaveImportSchemaFile(string defaultFileName);
    }

    public class FileDialogService : IFileDialogService
    {
        public string? OpenExcelFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Выберите файл Excel"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SavePhjsonFile(string defaultFileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PHJSON Files|*.phjson",
                FileName = defaultFileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? OpenImportSchemaFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Import Schema|*.phimportschema.json|JSON|*.json",
                Title = "Выберите шаблон схемы импорта"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SaveImportSchemaFile(string defaultFileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Import Schema|*.phimportschema.json|JSON|*.json",
                FileName = defaultFileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }

    public interface IMessageDialogService
    {
        void ShowInformation(string message, string title);

        void ShowWarning(string message, string title);

        void ShowError(string message, string title);
    }

    public class MessageDialogService : IMessageDialogService
    {
        public void ShowInformation(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
