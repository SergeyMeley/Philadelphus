using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Text;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Основная сущность Чубушника
    /// </summary>
    public abstract class MainEntityBaseModel : IMainEntityWritableModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected virtual string _defaultFixedPartOfName => "Новая основная сущность";

        private string _name;
        private string? _description;
        private bool _isHidden = false;
        private State _state = State.Initialized;

        #endregion

        #region [ Properties ]

        #region [ General Properties ]

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid { get; }

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
                if (_name != value)
                {
                    _name = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfoModel AuditInfo { get; set; } = new AuditInfoModel();

        /// <summary>
        /// Скрыт от нового использования (устаревший для бизнеса)
        /// </summary>
        public bool IsHidden
        {
            get
            {
                return _isHidden;
            }
            set
            {
                if (_isHidden != value)
                {
                    _isHidden = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Состояние
        /// </summary>
        public State State 
        { 
            get
            {
                return _state; 
            }
        }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public abstract IDataStorageModel DataStorage { get; }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Основная сущность Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal MainEntityBaseModel(
            Guid uuid, 
            IMainEntity dbEntity)
        {
            if (uuid == Guid.Empty)
                throw new ArgumentNullException(nameof(uuid));
            if (dbEntity == null)
                throw new ArgumentNullException(nameof(dbEntity));

            Uuid = uuid;
            Name = NamingHelper.GetNewName(new List<string>(), _defaultFixedPartOfName);
            DbEntity = dbEntity;    // TODO: Исключить и применить автомаппер
        }

        #endregion

        #region [ Methods ]

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

        /// <summary>
        /// Пересчитать статус при изменении значений свойств
        /// </summary>
        protected bool UpdateStateStateAfterChange()
        {
            if (_state != State.Initialized
                && _state != State.ForHardDelete
                && _state != State.ForHardDelete)
            {
                _state = State.Changed;
                return true;
            }
            return false;
        }

        #endregion

        #region OLD     

        /// <summary>
        /// Сущность БД
        /// </summary>
        public IMainEntity DbEntity { get; set; }   // TODO: Исключить и применить автомаппер

        #endregion
    }
}
