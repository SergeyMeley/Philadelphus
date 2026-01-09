using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Сервис работы с репозиторием, его участниками и содержимым
    /// </summary>
    public interface ITreeRepositoryService
    {
        #region [ Get + Load ]

        /// <summary>
        /// Принудительная загрузка из хранилища участников и содержимого репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns>Репозиторий с участниками и содержимым</returns>
        public TreeRepositoryModel ForceLoadTreeRepositoryMembersCollection(TreeRepositoryModel repository);

        /// <summary>
        /// Получить участников и содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <returns></returns>
        public TreeRepositoryModel GetTreeRepositoryMembersAndContent(TreeRepositoryModel repository);

        /// <summary>
        /// Получить участников и содержимое корня
        /// </summary>
        /// <param name="root">Корень</param>
        /// <returns>Корень с участниками и содержимым</returns>
        public TreeRootModel GetTreeRootMembersAndContent(TreeRootModel root);

        /// <summary>
        /// Получить участников и содержимое узла
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Узел с участниками и содержимым</returns>
        public TreeNodeModel GetTreeNodeMembersAndContent(TreeNodeModel node);

        /// <summary>
        /// Получить содержимое листа
        /// </summary>
        /// <param name="leave"></param>
        /// <returns>Лист с содержимым</returns>
        public TreeLeaveModel GetTreeLeaveContent(TreeLeaveModel leave);

        /// <summary>
        /// Получить атрибуты элемента
        /// </summary>
        /// <param name="attributeOwner">Владелец</param>
        /// <returns>Владелец с атрибутами</returns>
        public IEnumerable<ElementAttributeModel> GetPersonalAttributes(IAttributeOwnerModel attributeOwner);

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="treeRepository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(TreeRepositoryModel treeRepository, SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (корни)
        /// </summary>
        /// <param name="treeRoots">Коллекция корней</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeRootModel> treeRoots, SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (узлы)
        /// </summary>
        /// <param name="treeNodes">Коллекция узлов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeNodeModel> treeNodes, SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (листы)
        /// </summary>
        /// <param name="treeLeaves">Коллекция листов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(IEnumerable<TreeLeaveModel> treeLeaves, SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (атрибуты элемента)
        /// </summary>
        /// <param name="elementAttributes">Коллекция атрибутов</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveContentChanges(IEnumerable<ElementAttributeModel> elementAttributes);

        #endregion

        #region [ Create + Add ]

        public TreeRootModel CreateTreeRoot(TreeRepositoryModel parentElement, IDataStorageModel dataStorage);
        public TreeNodeModel CreateTreeNode(IParentModel parentElement);
        public TreeLeaveModel CreateTreeLeave(TreeNodeModel parentElement);
        public ElementAttributeModel CreateElementAttribute(IAttributeOwnerModel owner);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        public bool SoftDeleteRepositoryMember(IChildrenModel element);

        #endregion
    }
}
