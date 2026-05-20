using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Основная сущность Чубушника
    /// </summary>
    public abstract class MainEntityBaseModel<T> : IMainEntityWritableModel
        where T : MainEntityBaseModel<T>
    {
        #region [ Fields ]

        protected readonly INotificationService _notificationService;

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected virtual string _defaultFixedPartOfName => "Новая основная сущность";

        protected IPropertiesPolicy<T> _propertiesPolicy;
        private static readonly AsyncLocal<PropertiesPolicyContext> _context = new();

        private Guid _uuid;
        protected string _name;
        protected string? _description;
        private bool _isHidden = false;
        private State _state = State.Initialized;

        #endregion

        #region [ Properties ]

        #region [ General Properties ]

        /// <summary>
        /// Тип основной сущности.
        /// </summary>
        [Display(Name = "Тип", Description = "Тип сущности - репозиторий, кустарник, рабочее дерево, корень, узел или лист")]
        public string Type
        {
            get => typeof(T).Name;
        }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        [Display(Name = "UUID", Description = "Уникальный идентификатор UUID")]
        public Guid Uuid
        {
            get => GetValue(_uuid);
            private init => SetValue(ref _uuid, value);
        }

        /// <summary>
        /// Наименование
        /// </summary>
        [Display(Name = "Наименование", Description = "Наименование")]
        public string Name
        {
            get => GetValue(_name);
            set => SetValue(ref _name, value);
        }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Описание", Description = "Описание")]
        public string? Description
        {
            get => GetValue(_description);
            set => SetValue(ref _description, value);
        }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        [Display(Name = "Аудит", Description = "Информация для аудита")]
        public AuditInfoModel AuditInfo { get; set; } = new AuditInfoModel();

        /// <summary>
        /// Скрыт от нового использования (устаревший для бизнеса)
        /// </summary>
        [Display(Name = "Скрыто", Description = "Скрыто")]
        public bool IsHidden
        {
            get => GetValue(_isHidden);
            set => SetValue(ref _isHidden, value);
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Состояние
        /// </summary>
        [Display(Name = "Состояние", Description = "Состояние элемента - создано, изменено, удалено или сохранено")]
        public State State
        {
            get => GetValue(_state);
        }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        [Display(Name = "Хранилище", Description = "Хранилище данных")]
        public abstract IDataStorageModel DataStorage { get; }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Основная сущность Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        internal MainEntityBaseModel(
            Guid uuid,
            INotificationService notificationService,
            IPropertiesPolicy<T> propertiesPolicy)
        {
            _notificationService = notificationService;
            _propertiesPolicy = propertiesPolicy;

            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            _uuid = uuid;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Назначить статус
        /// </summary>
        /// <param name="newState">Новый статус</param>
        /// <returns>Результат выполнения операции.</returns>
        bool IMainEntityWritableModel.SetState(State newState)
        {
            if (_state == State.Initialized
                && newState != State.SavedOrLoaded)
            {
                return false;
            }
            _state = newState;
            return true;
        }

        /// <summary>
        /// Пересчитать статус при изменении значений свойств
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        protected bool UpdateStateStateAfterChange()
        {
            if (_state != State.Initialized
                && _state != State.ForSoftDelete
                && _state != State.ForHardDelete)
            {
                _state = State.Changed;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Присвоить автоматически сгенерированное наименование
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public string AssignAutoName()
        {
            Name = NamingHelper.GetNewName(fixedPartOfName: _defaultFixedPartOfName);
            return Name;
        }

        protected TValue GetValue<TValue>(
            TValue field,
            [CallerMemberName] string prop = null)
        {
            var ctx = _context.Value ??= new PropertiesPolicyContext();

            // Защита от зацикливания
            if (ctx.IsInProgress(this, field, prop))
                return field;

            try
            {
                ctx.Enter(this, field, prop);

                if (_propertiesPolicy?.CanRead((T)this, prop) == false)
                {
                    _notificationService.SendTextMessage<MainEntityBaseModel<T>>($"Ошибка получения значения свойства '{prop}' - нарушены ограничения системы.");
                    return default;
                }

                return (TValue)_propertiesPolicy?.OnRead((T)this, prop, field);
            }
            finally
            {
                ctx.Exit(this, field, prop);
            }
        }

        /// <summary>
        /// Получить значение свойства
        /// Проверяется на соответствие политикам
        /// </summary>
        /// <param name="field">Поле</param>
        /// <param name="prop">Свойство</param>
        /// <returns>Результат выполнения операции.</returns>
        protected bool SetValue<TValue>(
            ref TValue field,
            TValue value,
            [CallerMemberName] string prop = null)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return false;

            var ctx = _context.Value ??= new PropertiesPolicyContext();

            // Защита от зацикливания при записи
            if (ctx.IsWriteInProgress(this, prop))
                return false;

            try
            {
                ctx.EnterWrite(this, prop);

                if (_propertiesPolicy != null)
                {
                    value = (TValue)_propertiesPolicy.PrepareWriteValue((T)this, prop, value);
                }

                if (_propertiesPolicy?.CanWrite((T)this, prop, value) == false)
                {
                    _notificationService.SendTextMessage<MainEntityBaseModel<T>>(
                        $"Ошибка присвоения значения свойству '{prop}' - нарушены ограничения системы.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);
                    return false;
                }

                var oldValue = field;

                field = value;

                _propertiesPolicy?.OnWrite((T)this, prop, oldValue, value);

                UpdateStateStateAfterChange();

                return true;
            }
            finally
            {
                ctx.ExitWrite(this, prop);
            }
        }

        /// <summary>
        /// Заменяет политику свойств уже созданной модели.
        /// </summary>
        /// <remarks>
        /// Основной сценарий - загрузка из хранилища. Во время AutoMapper-маппинга модели создаются с
        /// <see cref="EmptyPropertiesPolicy{T}" />, чтобы старые или неконсистентные данные из БД можно было
        /// прочитать без срабатывания новых правил. После завершения маппинга сервис назначает рабочую
        /// политику, и дальнейшие пользовательские изменения снова проходят обычные проверки.
        /// </remarks>
        /// <param name="propertiesPolicy">Политика, которая будет применяться к последующим чтениям и записям.</param>
        internal void SetPropertiesPolicy(IPropertiesPolicy<T> propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(propertiesPolicy);

            _propertiesPolicy = propertiesPolicy;
        }

        #endregion
    }
}
