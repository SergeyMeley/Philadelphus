using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Корень дерева участников репозитория Чубушника (аналог проекта / библиотеки в .NET)
    /// </summary>
    public class  TreeRootModel : WorkingTreeMemberBaseModel<TreeRootModel>, IParentModel, IOwnerModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый корень";

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Системный корень
        /// </summary>
        public bool IsSystemBase { get; } = false;

        /// <summary>
        /// Уникальный идентификатор системного корня
        /// </summary>
        internal static Guid SystemBaseUuid { get => Guid.Parse("00000000-0000-0000-0000-000018151520"); }

        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Дочерние узлы репозитория Чубушника
        /// </summary>
        public List<TreeNodeModel> ChildNodes { get; }

        /// <summary>
        /// Наследники
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> Childs
        {
            get
            {
                var result = new Dictionary<Guid, IChildrenModel>();

                if (ChildNodes != null)
                {
                    foreach (var node in ChildNodes)
                    {
                        result.Add(node.Uuid, node);
                    }
                }
                
                return result.AsReadOnly();
            }
        }

        /// <summary>
        /// Все наследники (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> AllChildsRecursive { get; }

        #endregion

        #region [ Ownership Properties ]



        #endregion

        #region [ Infrastructure Properties ]



        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Корень дерева участников репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="owner">Владелец</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeRootModel(
            Guid uuid,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeRootModel> propertiesPolicy)
            : base(uuid, owner, notificationService, propertiesPolicy)
        {
            owner.ContentRoot = this;

            ChildNodes = new List<TreeNodeModel>();

            if (uuid == SystemBaseUuid)
            {
                IsSystemBase = true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        public bool AddChild(IChildrenModel child)
        {
            if (child is TreeNodeModel n
                && ChildNodes.Any(x => x.Uuid == child.Uuid) == false)
            {
                ChildNodes.Add(n);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить наследника
        /// </summary>
        /// <param name="child">Наследник</param>
        public bool RemoveChild(IChildrenModel child)
        {
            if (child is TreeNodeModel n
                && ChildNodes.Any(x => x.Uuid == child.Uuid))
            {
                var remItem = ChildNodes.First(x => x.Uuid == child.Uuid);
                ChildNodes.Remove(remItem);
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Очистить список наследников
        /// </summary>
        public bool ClearChilds()
        {
            ChildNodes.Clear();
            return true;
        }

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected override bool AddContentDetailed(IContentModel content)
        {
            return true;
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected override bool RemoveContentDetailed(IContentModel content)
        {
            return true;
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        protected override bool ClearContentDetailed()
        {
            return true;
        }

        #endregion
    }
}
