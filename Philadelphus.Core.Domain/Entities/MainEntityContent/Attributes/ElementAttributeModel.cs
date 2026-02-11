using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    /// <summary>
    /// Атрибут элемента Чубушника
    /// </summary>
    public class ElementAttributeModel : MainEntityBaseModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый атрибут";

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Тип данных (узел дерева репозитория Чубушника)
        /// </summary>
        public TreeNodeModel ValueType { get; set; }

        /// <summary>
        /// Коллекция типов данных (узлы дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeNodeModel>? ValueTypesList { get; set; }

        /// <summary>
        /// Значение (лист выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public TreeLeaveModel Value { get; set; }

        /// <summary>
        /// Коллекция допустимых значений (листы выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeLeaveModel>? ValuesList { get; set; }

        /// <summary>
        /// Область видимости
        /// </summary>
        public VisibilityScope Visibility { get; set; } = VisibilityScope.Public;

        /// <summary>
        /// Возможность переопределения
        /// </summary>
        public OverrideType Override { get; set; } = OverrideType.None;

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Владелец
        /// </summary>
        public IOwnerModel Owner { get; }

        /// <summary>
        /// Все владельцы (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IOwnerModel> AllOwnersRecursive 
        { 
            get => throw new NotImplementedException(); 
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage
        {
            get
            {
                if (Owner is IMainEntityModel m)
                {
                    return m.DataStorage;
                }
                return null;
            }
        }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Атрибут элемента Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="owner">Владелец атрибута</param>
        /// <param name="dbEntity">Сущность БД</param>
        public ElementAttributeModel(
            Guid uuid, 
            IOwnerModel owner, 
            IMainEntity dbEntity) 
            : base(uuid, dbEntity)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            Owner = owner;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить владельца
        /// </summary>
        public bool ChangeOwner(IOwnerModel newOwner)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
