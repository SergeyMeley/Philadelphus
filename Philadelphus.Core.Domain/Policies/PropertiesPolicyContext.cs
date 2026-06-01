using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies
{
    /// <summary>
    /// Контекст данных для политик свойств.
    /// </summary>
    internal class PropertiesPolicyContext
    {
        private readonly object _syncRoot = new();
        private readonly HashSet<(object, object, string)> _readingProps = new();
        private readonly HashSet<(object, string)> _writingProps = new();

        /// <summary>
        /// Выполняет операцию Enter.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="field">Поле.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool Enter(object model, object field, string prop)
        {
            lock (_syncRoot)
            {
                return _readingProps.Add((model, field, prop));
            }
        }

        /// <summary>
        /// Выполняет операцию Exit.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="field">Поле.</param>
        /// <param name="prop">Свойство.</param>
        public void Exit(object model, object field, string prop)
        {
            lock (_syncRoot)
            {
                _readingProps.Remove((model, field, prop));
            }
        }

        /// <summary>
        /// Признак выполняющейся операции.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="field">Поле.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool IsInProgress(object model, object field, string prop)
        {
            lock (_syncRoot)
            {
                return _readingProps.Contains((model, field, prop));
            }
        }

        /// <summary>
        /// Выполняет операцию EnterWrite.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool EnterWrite(object model, string prop)
        {
            lock (_syncRoot)
            {
                return _writingProps.Add((model, prop));
            }
        }

        /// <summary>
        /// Выполняет операцию ExitWrite.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        public void ExitWrite(object model, string prop)
        {
            lock (_syncRoot)
            {
                _writingProps.Remove((model, prop));
            }
        }

        /// <summary>
        /// Признак выполняющейся записи.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool IsWriteInProgress(object model, string prop)
        {
            lock (_syncRoot)
            {
                return _writingProps.Contains((model, prop));
            }
        }
    }
}
