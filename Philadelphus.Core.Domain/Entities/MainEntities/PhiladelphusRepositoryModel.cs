using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Репозиторий Чубушника (агрегатор корней, аналог решения в .NET)
    /// </summary>
    public class PhiladelphusRepositoryModel : MainEntityBaseModel<PhiladelphusRepositoryModel>, IPhiladelphusRepositoryHeaderModel, IOwnerModel, IHavingOwnDataStorageModel
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
                    UpdateStateStateAfterChange();
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
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Список псевдонимов репозитория
        /// </summary>
        [Display(Name = "[Псевдонимы]", Description = "Коллекция применяемых псевдонимов")]
        public Dictionary<string, object> AliasesList { get; } = new Dictionary<string, object>();

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Кустарник рабочих деревьев
        /// </summary>
        [Display(Name = "[Кустарник]", Description = "Содержащийся кустарник")]
        public ShrubModel ContentShrub { get; }

        /// <summary>
        /// Содержимое
        /// </summary>
        [Display(Name = "[Содержимое]", Description = "Содержимое")]
        public ReadOnlyDictionary<Guid, IContentModel> Content 
        {
            get => new Dictionary<Guid, IContentModel>(
                new[] { new KeyValuePair<Guid, IContentModel>(ContentShrub.Uuid, ContentShrub) })
                .AsReadOnly(); 
        }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        [Display(Name = "[Все содержимое]", Description = "Все содержимое")]
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => RecursiveRelationshipHelper.ToReadOnlyDictionary(
                RecursiveRelationshipHelper.EnumerateContentRecursive(this)); 
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        [Display(Name = "[Собственное хранилище]", Description = "Собственное хранилище данных")]
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
        [Display(Name = "[Наименование собственного хранилища]", Description = "Наименование собственного хранилища данных")]
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
        [Display(Name = "[UUID собственного хранилища]", Description = "UUID собственного хранилища данных")]
        public Guid OwnDataStorageUuid 
        { 
            get => _ownDataStorage.Uuid; 
        }

        /// <summary>
        /// Коллекция допустимых хранилищ данных дочерних элементов
        /// </summary>
        [Display(Name = "[Хранилища]", Description = "Хранилища данных")]
        public List<IDataStorageModel> DataStorages { get; } = new List<IDataStorageModel>();

        /// <summary>
        /// Хранилище данных по умолчанию для участников кустарника.
        /// </summary>
        [Display(Name = "[Хранилище кустарника по умолчанию]", Description = "Хранилище данных кустарника по умолчанию")]
        public IDataStorageModel? DefaultShrubMembersDataStorage { get; internal set; }

        /// <summary>
        /// Хранилище данных по умолчанию для отчетов.
        /// </summary>
        [Display(Name = "[Хранилище отчетов по умолчанию]", Description = "Хранилище данных отчетов по умолчанию")]
        public IDataStorageModel? DefaultReportsDataStorage { get; internal set; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        [Display(Name = "[Хранилище]", Description = "Хранилище данных")]
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
        internal PhiladelphusRepositoryModel(
            Guid uuid, 
            IDataStorageModel dataStorage,
            INotificationService notificationService,
            IPropertiesPolicy<PhiladelphusRepositoryModel> propertiesPolicy,
            IPropertiesPolicy<ShrubModel> shrubPropertiesPolicy)
            : base(uuid, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(dataStorage);

            OwnDataStorage = dataStorage;

            ContentShrub = new ShrubModel(uuid, this, notificationService, shrubPropertiesPolicy);

        }

        #endregion

        #region[ Methods ]

        /// <summary>
        /// Изменить хранилище данных (не реализовано)
        /// </summary>
        /// <param name="storage">Новое хранилище</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            ArgumentNullException.ThrowIfNull(storage);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool AddContent(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            return false;
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool RemoveContent(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            return false;
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool ClearContent()
        {
            return false;
        }

        #endregion
    }
}
