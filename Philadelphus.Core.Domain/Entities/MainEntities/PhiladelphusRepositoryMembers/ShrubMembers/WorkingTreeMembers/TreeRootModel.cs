using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Корень дерева участников репозитория Чубушника (аналог проекта / библиотеки в .NET)
    /// </summary>
    public class  TreeRootModel : WorkingTreeMemberBaseModel, IParentModel, IOwnerModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый корень";

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]



        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Дочерние узлы репозитория Чубушника
        /// </summary>
        public List<TreeNodeModel> ContentNodes { get; }

        /// <summary>
        /// Наследники
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> Childs
        {
            get
            {
                var result = new Dictionary<Guid, IChildrenModel>();

                if (ContentNodes != null)
                {
                    foreach (var node in ContentNodes)
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
            IMainEntity dbEntity)
            : base(uuid, owner, dbEntity)
        {
            ContentNodes = new List<TreeNodeModel>();
        }

        #endregion

        #region [ Methods ]



        #endregion
    }
}
