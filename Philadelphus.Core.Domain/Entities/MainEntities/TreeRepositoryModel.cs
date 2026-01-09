using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Text;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    /// <summary>
    /// Репозиторий Чубушника (агрегатор корней, аналог решения в .NET)
    /// </summary>
    public class TreeRepositoryModel : ITreeRepositoryHeaderModel, IHavingOwnDataStorageModel, IParentModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected virtual string DefaultFixedPartOfName { get => "Новый репозиторий"; }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        private Guid _uuid;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Uuid 
        { 
            get
            {
                return _uuid;
            }
            protected set
            {
                if (_uuid != value)
                {
                    _uuid = value;
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

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
                if (_name != value)
                {
                    _name = value;
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

        /// <summary>
        /// Описание
        /// </summary>
        private string _description;

        /// <summary>
        /// Описание
        /// </summary>
        public string Description
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
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfoModel AuditInfo { get; set; } = new AuditInfoModel();

        /// <summary>
        /// Состояние
        /// </summary>
        public State State { get; internal set; } = State.Initialized;

        /// <summary>
        /// Сущность БД
        /// </summary>
        public TreeRepository DbEntity { get; set; }

        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        private IDataStorageModel _ownDataStorage;

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
                    DataStorages.Add(value);
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

        /// <summary>
        /// Наименование собственного хранилища данных
        /// </summary>
        public string OwnDataStorageName { get => _ownDataStorage.Name; set => OwnDataStorageName = value; }    //TODO: Исправить костыль

        /// <summary>
        /// Уникальный идентификатор собственного хранилища данных
        /// </summary>
        public Guid OwnDataStorageUuid { get => _ownDataStorage.Uuid; set => OwnDataStorageUuid = value; }      //TODO: Исправить костыль

        /// <summary>
        /// Коллекция допустимых хранилищ данных дочерних элементов
        /// </summary>
        public List<IDataStorageModel> DataStorages { get; internal set; } = new List<IDataStorageModel>();

        //TODO
        /// <summary>
        /// Дочерние корни репозитория Чубушника
        /// </summary>
        public List<TreeRootModel> ChildTreeRoots { get => Childs.Where(x => x.GetType() == typeof(TreeRootModel)).Cast<TreeRootModel>().ToList(); }

        //TODO
        /// <summary>
        /// Уникальные идентификаторы дочерних элементов
        /// </summary>
        public List<Guid> ChildsUuids {  get; internal set; }

        /// <summary>
        /// Дочерние элементы
        /// </summary>
        public List<IChildrenModel> Childs { get; internal set; }

        /// <summary>
        /// Плоская коллекция элементов репозитория
        /// </summary>
        public List<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();

        /// <summary>
        /// Время последнего открытия пользователем репозитория Чубушника
        /// </summary>
        public DateTime? LastOpening { get; set; }

        /// <summary>
        /// Избранный
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Репозиторий Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeRepositoryModel(Guid uuid, IDataStorageModel dataStorage, TreeRepository dbEntity)
        {
            Uuid = uuid;
            OwnDataStorage = dataStorage;
            DbEntity = dbEntity;
            Initialize();
        }

        /// <summary>
        /// Репозиторий Чубушника
        /// </summary>
        /// <param name="headerModel">Заголовок репозитория</param>
        internal TreeRepositoryModel(TreeRepositoryHeaderModel headerModel)
        {
            Uuid = headerModel.Uuid;
            Initialize();
        }

        /// <summary>
        /// Получить репозиторий в виде строки
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
        /// Инициализировать
        /// </summary>
        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new string[0], DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
        }

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

    }
}
