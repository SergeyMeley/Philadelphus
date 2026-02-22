using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ImportFromExcelWindow.xaml
    /// </summary>
    public partial class ImportFromExcelWindow : Window
    {
        private readonly ConversionService _service;
        private string _selectedFilePath;
        public ImportFromExcelWindow()
        {
            InitializeComponent();
            _service = new ConversionService();
            InitializeRootsList();
        }


        private void InitializeRootsList()
        {
            // Загружаем корни из "хранилища"
            var roots = _service.GetExistingRootsFromStorage();
            CmbExistingRoots.ItemsSource = roots;
            if (roots.Count > 0) CmbExistingRoots.SelectedIndex = 0;
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Выберите файл Excel"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedFilePath = dialog.FileName;
                TxtFilePath.Text = Path.GetFileName(_selectedFilePath);

                // ТЗ: По умолчанию заполняется значением, равным наименованию книги Excel
                TxtRootName.Text = Path.GetFileNameWithoutExtension(_selectedFilePath);
            }
        }

        private void ChkCreateNewRoot_Checked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = true;
            CmbExistingRoots.IsEnabled = false;
        }

        private void ChkCreateNewRoot_Unchecked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = false;
            CmbExistingRoots.IsEnabled = true;
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Сначала выберите файл Excel!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isNewRoot = ChkCreateNewRoot.IsChecked == true;
            string rootName = isNewRoot ? TxtRootName.Text : CmbExistingRoots.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(rootName))
            {
                MessageBox.Show("Укажите наименование корня!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Запуск конвертации
                var jsonObject = _service.ProcessFile(_selectedFilePath, isNewRoot, rootName);

                // Сериализация в JSON
                var jsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
                string jsonResult = JsonConvert.SerializeObject(jsonObject, jsonSettings);

                // Сохранение результата
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON Files|*.json",
                    FileName = Path.GetFileNameWithoutExtension(_selectedFilePath) + ".json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, jsonResult);
                    MessageBox.Show("Файл успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
