using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Правило, ограничивающее значения свойства Name.
    /// </summary>
    /// <remarks>Implements requirements R-1.01, R-1.02, R-1.03, R-1.04, R-1.05 and R-1.006.</remarks>
    internal class ValidNamePropertiesRule<T> : IPropertiesRule<T>, IPrepareWriteValuePropertiesRule<T>, IAttributePropertiesRule<ElementAttributeModel>
        where T : MainEntityBaseModel<T>
    {
        private readonly INotificationService _notificationService;
        private readonly INameUniquenessStrategy<T> _strategy;

        /// <summary>
        /// Инициализирует правило проверки свойства <c>Name</c>.
        /// </summary>
        /// <param name="notificationService">Сервис пользовательских уведомлений.</param>
        /// <param name="strategy">
        /// Стратегия, определяющая область уникальности имени и набор типов, имена свойств которых зарезервированы.
        /// Одна реализация правила используется для разных моделей, а различия между ними вынесены в стратегию.
        /// </param>
        public ValidNamePropertiesRule(
            INotificationService notificationService,
            INameUniquenessStrategy<T> strategy)
        {
            _notificationService = notificationService;
            _strategy = strategy;
        }

        public bool CanRead(T model, string prop)
        {
            return true;
        }

        public bool CanWrite(T model, string prop, object value)
        {
            if (prop != nameof(MainEntityBaseModel<T>.Name)
                || value is not string newName)
            {
                return true;
            }

            // Унаследованный атрибут получает Name от родительского атрибута.
            // Его изменение контролирует NonOwnAttributePropertiesRule, поэтому здесь не проверяем его повторно.
            if (model is ElementAttributeModel { IsOwn: false })
            {
                return true;
            }

            // Запрещаем пользовательские имена, совпадающие с системными свойствами модели.
            // Это защищает табличные представления и импорт/экспорт, где имя атрибута может использоваться как ключ.
            var reservedNames = _strategy.ReservedPropertyTypes
                .SelectMany(x => x.GetProperties())
                .SelectMany(GetReservedPropertyNames)
                .ToHashSet(StringComparer.Ordinal);

            if (reservedNames.Contains(newName))
            {
                SendRestrictionNotification(model, prop, $"наименование '{newName}' совпадает с наименованием системного свойства");
                return false;
            }

            // Стратегия возвращает только те элементы, с которыми текущая модель должна делить область имен:
            // для узла это непосредственное содержимое родителя, для атрибута - содержимое его владельца,
            // для рабочего дерева - деревья внутри ShrubModel.
            if (_strategy.GetNamedItems(model).Any(x => x.Uuid != model.Uuid && x.Name == newName))
            {
                SendRestrictionNotification(model, prop, $"у текущего владельца уже есть другой элемент с наименованием '{newName}'");
                return false;
            }

            return true;
        }

        public object PrepareWriteValue(T model, string prop, object value)
        {
            if (prop != nameof(MainEntityBaseModel<T>.Name)
                || value is not string newName)
            {
                return value;
            }

            if (model is ElementAttributeModel { IsOwn: false })
            {
                return value;
            }

            // Невалидные символы не делают запись ошибочной: пользовательское значение мягко нормализуется,
            // а отдельное RequiredNamePropertiesRule затем проверяет, что после нормализации имя не стало пустым.
            var normalizedName = NormalizeName(newName);
            var invalidCharacters = GetInvalidNameCharacters(newName).ToList();
            if (invalidCharacters.Count > 0)
            {
                _notificationService.SendTextMessage<ValidNamePropertiesRule<T>>(
                    $"Из наименования элемента '{model.Name}' [{model.Uuid}] удалены недопустимые символы: {FormatCharacters(invalidCharacters)}.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
            }

            return normalizedName;
        }

        /// <summary>
        /// Нормализует имя так же, как это делает setter модели: удаляет запрещенные символы и обрезает края.
        /// </summary>
        /// <remarks>
        /// Метод используется не только при записи свойства, но и при импорте JSON для поиска уже созданных
        /// атрибутов по имени. Это важно, потому что имя из файла и фактическое имя в модели должны проходить
        /// одинаковое преобразование.
        /// </remarks>
        /// <param name="value">Исходное имя.</param>
        /// <returns>Имя без запрещенных символов и лишних пробелов по краям.</returns>
        internal static string NormalizeName(string? value)
        {
            if (value == null)
                return string.Empty;

            var valueWithoutInvalidCharacters = new string(value.Where(x => IsInvalidNameCharacter(x) == false).ToArray());

            return CollapseSpaces(valueWithoutInvalidCharacters.Trim());
        }

        public object OnRead(T model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(T model, string prop, object oldValue, object newValue)
        {
        }

        bool IAttributePropertiesRule<ElementAttributeModel>.CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        bool IAttributePropertiesRule<ElementAttributeModel>.CanWrite(ElementAttributeModel model, string prop, object value)
        {
            return model is T typedModel
                ? CanWrite(typedModel, prop, value)
                : true;
        }

        object IAttributePropertiesRule<ElementAttributeModel>.OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        void IAttributePropertiesRule<ElementAttributeModel>.OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }

        private static IEnumerable<char> GetInvalidNameCharacters(string value)
        {
            return value.Where(IsInvalidNameCharacter).Distinct();
        }

        private static IEnumerable<string> GetReservedPropertyNames(System.Reflection.PropertyInfo property)
        {
            yield return property.Name;

            var displayName = property.GetCustomAttributes(typeof(DisplayAttribute), inherit: true)
                .OfType<DisplayAttribute>()
                .FirstOrDefault()
                ?.Name;

            if (string.IsNullOrWhiteSpace(displayName) == false)
            {
                yield return displayName;
            }
        }

        private static bool IsInvalidNameCharacter(char value)
        {
            // Список намеренно является списком запрещенных символов, а не белым списком допустимых.
            // Требование: удалять только явно перечисленные символы, оставляя остальные пользовательские
            // символы имени без изменений.
            return value == '{'
                || value == '}'
                || value == '['
                || value == ']'
                || value == '~'
                || value == '&';
        }

        private static string CollapseSpaces(string value)
        {
            if (value.Length == 0)
                return value;

            var result = new System.Text.StringBuilder(value.Length);
            var previousWasSpace = false;

            foreach (var character in value)
            {
                if (character == ' ')
                {
                    if (previousWasSpace)
                    {
                        continue;
                    }

                    previousWasSpace = true;
                }
                else
                {
                    previousWasSpace = false;
                }

                result.Append(character);
            }

            return result.ToString();
        }

        private static string FormatCharacters(IEnumerable<char> values)
        {
            return string.Join(", ", values.Select(x => $"'{x}'"));
        }

        private void SendRestrictionNotification(T model, string prop, string reason)
        {
            _notificationService.SendTextMessage<ValidNamePropertiesRule<T>>(
                $"Для элемента '{model.Name}' [{model.Uuid}] изменение значения свойства '{prop}' ограничено, т.к. {reason}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }
    }
}
