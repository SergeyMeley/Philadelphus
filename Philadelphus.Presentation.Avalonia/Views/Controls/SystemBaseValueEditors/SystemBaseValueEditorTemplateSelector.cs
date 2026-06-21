using System.Reflection;

using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;

using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.SystemBaseValueEditors
{
    /// <summary>
    /// Выбирает шаблон редактора значения базового листа по его <see cref="SystemBaseType"/>.
    /// Avalonia-аналог WPF-стиля SystemBaseValueEditorContentControlStyle с DataTrigger'ами:
    /// в Avalonia нет триггеров стиля, поэтому выбор шаблона вынесен в IDataTemplate-селектор.
    /// Содержимое (Content) должно предоставлять свойство SystemBaseType и свойства,
    /// используемые выбранным шаблоном (StringValue, TimeValue и т.д.).
    /// </summary>
    /// <remarks>
    /// TODO: Тех. долг. Типизированные редакторы DATE/DATETIME (DatePicker) пока заменены
    /// строковым редактором (правка StringValue) — у Avalonia DatePicker.SelectedDate тип
    /// DateTimeOffset?, что требует отдельного согласования с DateValue (DateTime?).
    /// </remarks>
    public class SystemBaseValueEditorTemplateSelector : IDataTemplate
    {
        /// <summary>Шаблон для строковых и прочих неспециализированных типов (STRING, BOOL, OBJECT, …).</summary>
        public IDataTemplate? StringTemplate { get; set; }

        /// <summary>Шаблон для числовых типов (INTEGER, NUMERIC, FLOAT, MONEY).</summary>
        public IDataTemplate? NumericTemplate { get; set; }

        /// <summary>Шаблон для времени (TIME).</summary>
        public IDataTemplate? TimeTemplate { get; set; }

        /// <summary>Шаблон для файла (FILE).</summary>
        public IDataTemplate? FileTemplate { get; set; }

        public bool Match(object? data) => true;

        public Control? Build(object? param)
        {
            var template = SelectTemplate(param) ?? StringTemplate;
            return template?.Build(param);
        }

        private IDataTemplate? SelectTemplate(object? data)
        {
            if (data == null)
            {
                return StringTemplate;
            }

            var property = data.GetType().GetProperty(nameof(SystemBaseType), BindingFlags.Public | BindingFlags.Instance);
            if (property?.GetValue(data) is not SystemBaseType systemBaseType)
            {
                return StringTemplate;
            }

            return systemBaseType switch
            {
                SystemBaseType.INTEGER
                    or SystemBaseType.NUMERIC
                    or SystemBaseType.FLOAT
                    or SystemBaseType.MONEY => NumericTemplate,
                SystemBaseType.TIME => TimeTemplate,
                SystemBaseType.FILE => FileTemplate,
                // DATE/DATETIME пока редактируются как строка (см. TODO в summary класса).
                _ => StringTemplate,
            };
        }
    }
}
