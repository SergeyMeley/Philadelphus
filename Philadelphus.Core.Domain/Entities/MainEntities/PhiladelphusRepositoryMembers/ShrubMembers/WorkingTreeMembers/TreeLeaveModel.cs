using Microsoft.Extensions.Primitives;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Лист дерева участников репозитория Чубушника (аналог объекта в ООП)
    /// </summary>
    public class  TreeLeaveModel : WorkingTreeMemberBaseModel<TreeLeaveModel>, IWorkingTreeMemberModel, IChildrenModel, IOwnerModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый лист";

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        public virtual SystemBaseType SystemBaseType { get => SystemBaseType.USER_DEFINED; }

        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Родитель
        /// </summary>
        public TreeNodeModel ParentNode { get; }

        /// <summary>
        /// Родитель
        /// </summary>
        public IParentModel Parent { get => ParentNode; }

        /// <summary>
        /// Все родители (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IOwnerModel> AllParentsRecursive
        {
            get => throw new NotImplementedException();
        }

        #endregion

        #region [ Ownership Properties ]

        #endregion

        #region [ Infrastructure Properties ]



        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Лист репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский узел Чубушника</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeLeaveModel(
            Guid uuid,
            TreeNodeModel parent,
            WorkingTreeModel owner,
            INotificationService notificationService)
            : base(uuid, owner, notificationService)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            ParentNode = parent;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить родителя
        /// </summary>
        public bool ChangeParent(IParentModel newParent)
        {
            return false;
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
