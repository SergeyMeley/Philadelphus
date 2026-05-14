using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs
{
    /// <summary>
    /// Модель представления для конфигурационного файла.
    /// </summary>
    public class ConfigurationFileVM : ViewModelBase
    {
        /// <summary>
        /// Сведения о файле.
        /// </summary>
        public FileInfo FileInfo { get; private set; }
      
        /// <summary>
        /// Имя конфигурации.
        /// </summary>
        public string ConfigName { get; init; }
      
        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string FilePath => FileInfo.FullName;
       
        /// <summary>
        /// Признак существования.
        /// </summary>
        public bool Exists { get => FileInfo.Exists; }
       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ConfigurationFileVM" />.
        /// </summary>
        /// <param name="name">Наименование.</param>
        /// <param name="fileInfo">Параметр fileInfo.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="ArgumentException">Если строковый аргумент равен null, пустой строке или состоит только из пробельных символов.</exception>
        public ConfigurationFileVM(string name, FileInfo fileInfo)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(fileInfo);

            ConfigName = name;
            FileInfo = fileInfo;
        }
    
        /// <summary>
        /// Выполняет операцию Equals.
        /// </summary>
        /// <param name="obj">Параметр obj.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is ConfigurationFileVM cf && cf?.ConfigName == this.ConfigName)
                return true;
            return false;
        }

        /// <summary>
        /// Выполняет операцию ChangeFile.
        /// </summary>
        /// <param name="newFile">Параметр newFile.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool ChangeFile(FileInfo newFile)
        {
            ArgumentNullException.ThrowIfNull(newFile);

            FileInfo = newFile;
            OnPropertyChanged(nameof(FileInfo));
            return true;
        }
    }
}
