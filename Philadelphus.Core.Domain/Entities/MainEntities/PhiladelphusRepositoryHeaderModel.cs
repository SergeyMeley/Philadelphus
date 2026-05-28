using Philadelphus.Core.Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Заголовок репозитория Чубушника
    /// </summary>
    public class PhiladelphusRepositoryHeaderModel : IPhiladelphusRepositoryHeaderModel
    {
        #region [ Fields ]

        private string _name;
        private string? _description;
        private string _ownDataStorageName;
        private Guid _ownDataStorageUuid;
        private DateTime? _lastOpening;
        private bool _isFavorite = false;
        private bool _isHidden = false;
        private State _state = State.Initialized;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        [Display(Name = "[UUID]", Description = "Уникальный идентификатор UUID")]
        public Guid Uuid { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Display(Name = "[Наименование]", Description = "Наименование")]
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
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "[Описание]", Description = "Описание")]
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
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Наименование собственного хранилища данных
        /// </summary>
        [Display(Name = "[Наименование собственного хранилища]", Description = "Наименование собственного хранилища данных")]
        public string OwnDataStorageName
        {
            get
            {
                return _ownDataStorageName;
            }
            set
            {
                if (_ownDataStorageName != value)
                {
                    _ownDataStorageName = value;
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Уникальный идентификатор собственного хранилища данных
        /// </summary>
        [Display(Name = "[UUID собственного хранилища]", Description = "UUID собственного хранилища данных")]
        public Guid OwnDataStorageUuid
        {
            get
            {
                return _ownDataStorageUuid;
            }
            set
            {
                if (_ownDataStorageUuid != value)
                {
                    _ownDataStorageUuid = value;
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Время последнего открытия пользователем репозитория Чубушника
        /// </summary>
        [Display(Name = "[Последнее открытие]", Description = "Последнее открытие")]
        public DateTime? LastOpening
        {
            get
            {
                return _lastOpening;
            }
            set
            {
                if (_lastOpening != value)
                {
                    _lastOpening = value;
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Избранный
        /// </summary>
        [Display(Name = "[Избранное]", Description = "Избранное")]
        public bool IsFavorite
        {
            get
            {
                return _isFavorite;
            }
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Скрытый
        /// </summary>
        [Display(Name = "[Скрыто]", Description = "Скрыто")]
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
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Состояние
        /// </summary>
        [Display(Name = "[Состояние]", Description = "Состояние элемента - создано, изменено, удалено или сохранено")]
        public State State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    _state = value;
                }
            }
        }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Заголовок репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор.</param>
        internal PhiladelphusRepositoryHeaderModel(Guid uuid)
        {
            Uuid = uuid;
        }

        #endregion

        #region [ Methods ]

        protected bool UpdateStateAfterChange()
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
    }
}
