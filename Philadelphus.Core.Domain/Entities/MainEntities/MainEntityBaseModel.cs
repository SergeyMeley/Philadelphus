 using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Text;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Основная сущность Чубушника
    /// </summary>
    public abstract class MainEntityBaseModel : IMainEntityWritableModel, ILinkableByUuidModel
    {
        //TODO: Во всех IMainEntityModel при изменении значения свойств обновлять статус
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected virtual string DefaultFixedPartOfName { get => "Новая основная сущность"; }

        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public abstract EntityTypesModel EntityType { get; }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; protected set; }

        /// <summary>
        /// Наименование
        /// </summary>
        private string _name;

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name 
        { 
            get
            {
                return _name;
            }
            set
            {
                _name = value;
               if(_state != State.Initialized)
                    _state = State.Changed;
            }
        }

        /// <summary>
        /// Псевдоним
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Пользовательский код
        /// </summary>
        public string CustomCode { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Имеет атрибуты
        /// </summary>
        public bool HasAttributes { get; set; }

        /// <summary>
        /// Устаревший
        /// </summary>
        public bool IsLegacy { get; set; }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfoModel AuditInfo { get; private set; } = new AuditInfoModel();

        /// <summary>
        /// Тип элемента (устар.)
        /// </summary>
        public EntityElementTypeModel ElementType { get; set; }
        
        /// <summary>
        /// Состояние
        /// </summary>
        private State _state = State.Initialized;

        /// <summary>
        /// Состояние
        /// </summary>
        public State State { get => _state; }

        /// <summary>
        /// Сущность БД
        /// </summary>
        public IMainEntity DbEntity { get; set; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public abstract IDataStorageModel DataStorage { get; }

        /// <summary>
        /// Основная сущность Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="dbEntity">Сущность БД</param>
        public MainEntityBaseModel(Guid uuid, IMainEntity dbEntity)
        {
            DbEntity = dbEntity;
            Uuid = uuid;
        }

        /// <summary>
        /// Получить сущность в виде строки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.AppendLine();
            sb.Append(Uuid);
            return sb.ToString();
        }

        /// <summary>
        /// Назначить статус
        /// </summary>
        /// <param name="newState">Новый статус</param>
        /// <returns></returns>
        bool IMainEntityWritableModel.SetState(State newState)
        {
            _state = newState;
            return true;
        }
    }
}
