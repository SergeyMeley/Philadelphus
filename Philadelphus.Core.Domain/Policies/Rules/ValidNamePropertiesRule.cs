using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Правило, ограничивающее значения свойства Name.
    /// </summary>
    internal class ValidNamePropertiesRule<T> : IPropertiesRule<T>, IPrepareWriteValuePropertiesRule<T>, IAttributePropertiesRule<ElementAttributeModel>
        where T : MainEntityBaseModel<T>
    {
        private readonly INotificationService _notificationService;
        private readonly INameUniquenessStrategy<T> _strategy;

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

            if (model is ElementAttributeModel { IsOwn: false })
            {
                return true;
            }

            var reservedNames = _strategy.ReservedPropertyTypes
                .SelectMany(x => x.GetProperties())
                .Select(x => x.Name)
                .ToHashSet(StringComparer.Ordinal);

            if (reservedNames.Contains(newName))
            {
                SendRestrictionNotification(model, prop, $"наименование '{newName}' совпадает с наименованием системного свойства");
                return false;
            }

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

        internal static string NormalizeName(string? value)
        {
            if (value == null)
                return string.Empty;

            return new string(value.Where(x => IsInvalidNameCharacter(x) == false).ToArray()).Trim();
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

        private static bool IsInvalidNameCharacter(char value)
        {
            return value == '{'
                || value == '}'
                || value == '['
                || value == ']'
                || value == '~'
                || value == '&'
                || value == '%';
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
