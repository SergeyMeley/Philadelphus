using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Philadelphus.WpfPlugins.Interfaces
{
    public interface IPlugin
    {
        /// <summary>
        /// Наименование расширения.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Описание расширения.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Автоматическая инициализация расширения при запуске приложения.
        /// </summary>
        public bool AutoStart { get; }
        /// <summary>
        /// Запуск расширения.
        /// </summary>
        public void Start();
        /// <summary> 
        /// Останов расширения.
        /// </summary>
        public void Stop();
        /// <summary>
        /// Обработать текущий элемент репозитория.
        /// </summary>
        /// <param name="currentElement">Текущий выбранный элемент.</param>
        /// <returns></returns>
        public IMainEntityModel ProcessElement(IMainEntityModel currentElement);
        /// <summary>
        /// Проверить возможность запуска обработки.
        /// </summary>
        /// <param name="currentElement">Текущий выбранный элемент.</param>
        /// <param name="outMessage">Сообщение о причине невозможности обработки.</param>
        /// <returns></returns>
        public bool CheckConditions(IMainEntityModel currentElement, out string outMessage);
        /// <summary>
        /// Открыть инструкцию по работе.
        /// </summary>
        public void OpenInstruction();
        /// <summary>
        /// Виджет расширения для панели инструментов.
        /// </summary>
        public UserControl RibbonPluginView { get; }
        /// <summary>
        /// Виджет расширения для панели управления текущим элементом.
        /// </summary>
        public UserControl ElementDetailsPluginView { get; }
    }
}
