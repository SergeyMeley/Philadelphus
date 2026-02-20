using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Репозиторий Чубушника (агрегатор корней, аналог решения в .NET)
    /// </summary>
    public class PhiladelphusRepositoryModel : MainEntityBaseModel, IPhiladelphusRepositoryHeaderModel, IOwnerModel, IHavingOwnDataStorageModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый репозиторий";

        private DateTime? _lastOpening;
        private bool _isFavorite;
        private IDataStorageModel _ownDataStorage;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Время последнего открытия пользователем репозитория Чубушника
        /// </summary>
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
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Избранный
        /// </summary>
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
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Список псевдонимов репозитория
        /// </summary>
        public Dictionary<string, object> AliasesList { get; } = new Dictionary<string, object>();

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Кустарник рабочих деревьев
        /// </summary>
        public ShrubModel ContentShrub { get; }

        /// <summary>
        /// Содержимое
        /// </summary>
        public ReadOnlyDictionary<Guid, IContentModel> Content 
        {
            get => new Dictionary<Guid, IContentModel>(
                new[] { new KeyValuePair<Guid, IContentModel>(ContentShrub.Uuid, ContentShrub) })
                .AsReadOnly(); 
        }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => throw new NotImplementedException(); 
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        public IDataStorageModel OwnDataStorage
        {
            get
            {
                return _ownDataStorage;
            }
            private set
            {
                if (_ownDataStorage != value)
                {
                    _ownDataStorage = value;
                    if (DataStorages.Any(x => x.Uuid == value.Uuid) == false)
                        DataStorages.Add(value);
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Наименование собственного хранилища данных
        /// </summary>
        public string OwnDataStorageName
        {
            get
            {
                return OwnDataStorage.Name;
            }
            set
            {
                if (OwnDataStorage.Name != value)
                {
                    OwnDataStorage.Name = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Уникальный идентификатор собственного хранилища данных
        /// </summary>
        public Guid OwnDataStorageUuid 
        { 
            get => _ownDataStorage.Uuid; 
        }

        /// <summary>
        /// Коллекция допустимых хранилищ данных дочерних элементов
        /// </summary>
        public List<IDataStorageModel> DataStorages { get; } = new List<IDataStorageModel>();

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage 
        { 
            get => OwnDataStorage; 
        }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Репозиторий Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal PhiladelphusRepositoryModel(
            Guid uuid, 
            IDataStorageModel dataStorage, 
            PhiladelphusRepository dbEntity)
            : base(uuid, dbEntity)
        {
            if (dataStorage == null)
                throw new ArgumentNullException(nameof(dataStorage));

            OwnDataStorage = dataStorage;

            ContentShrub = new ShrubModel(uuid, dbEntity, this);
        }

        #endregion

        #region[ Methods ]

        /// <summary>
        /// Изменить хранилище данных (не реализовано)
        /// </summary>
        /// <param name="storage">Новое хранилище</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
