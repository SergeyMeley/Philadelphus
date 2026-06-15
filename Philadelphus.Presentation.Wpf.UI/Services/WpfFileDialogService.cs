using Microsoft.Win32;
using Philadelphus.Presentation.Services.Interfaces;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public class WpfFileDialogService : IFileDialogService
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

        public string? BrowseLocalFile()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? OpenFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false
            };

            ApplyCommonOptions(dialog, filter, defaultExtension, title, initialDirectory);

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SaveFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true
            };

            ApplyCommonOptions(dialog, filter, defaultExtension, title, initialDirectory);

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? BrowseFolder(string? title = null, string? initialDirectory = null)
        {
            var dialog = new OpenFolderDialog
            {
                Multiselect = false
            };

            if (string.IsNullOrEmpty(title) == false)
            {
                dialog.Title = title;
            }

            if (string.IsNullOrEmpty(initialDirectory) == false)
            {
                dialog.InitialDirectory = initialDirectory;
                dialog.DefaultDirectory = initialDirectory;
            }

            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        private static void ApplyCommonOptions(
            FileDialog dialog,
            string? filter,
            string? defaultExtension,
            string? title,
            string? initialDirectory)
        {
            if (string.IsNullOrEmpty(filter) == false)
            {
                dialog.Filter = filter;
            }

            if (string.IsNullOrEmpty(defaultExtension) == false)
            {
                dialog.DefaultExt = defaultExtension;
            }

            if (string.IsNullOrEmpty(title) == false)
            {
                dialog.Title = title;
            }

            if (string.IsNullOrEmpty(initialDirectory) == false)
            {
                dialog.InitialDirectory = initialDirectory;
                dialog.DefaultDirectory = initialDirectory;
            }
        }
    }
}
