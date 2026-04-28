using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Сервис работы с репозиторием, его участниками и содержимым
    /// </summary>
    public interface IPhiladelphusRepositoryService
    {
        #region [ Get + Load ]

        /// <summary>
        /// Получить участников и содержимое репозитория
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="force">Признак принудительного чтения из хранилища</param>
        /// <returns></returns>
        public PhiladelphusRepositoryModel GetShrubContent(PhiladelphusRepositoryModel repository, bool force = false);

        /// <summary>
        /// Получить участников и содержимое репозитория (асинхронно)
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="force">Признак принудительного чтения из хранилища</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Репозиторий с участниками и содержимым</returns>
        public Task<PhiladelphusRepositoryModel> GetShrubContentAsync(
            PhiladelphusRepositoryModel repository,
            bool force = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить рабочее дерево
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="force">Признак принудительного чтения из хранилища</param>
        /// <returns>Корень с содержимым</returns>
        public WorkingTreeModel GetWorkingTreeContent(WorkingTreeModel tree, bool force = false);

        /// <summary>
        /// Получить рабочее дерево (асинхронно)
        /// </summary>
        /// <param name="tree">Рабочее дерево</param>
        /// <param name="force">Признак принудительного чтения из хранилища</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Корень с содержимым</returns>
        public Task<WorkingTreeModel> GetWorkingTreeContentAsync(
            WorkingTreeModel tree,
            bool force = false,
            CancellationToken cancellationToken = default);

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="PhiladelphusRepository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(ref PhiladelphusRepositoryModel PhiladelphusRepository, SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (репозиторий, асинхронно)
        /// </summary>
        /// <param name="PhiladelphusRepository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Количество сохраненных изменений</returns>
        public Task<long> SaveChangesAsync(
            PhiladelphusRepositoryModel PhiladelphusRepository,
            SaveMode saveMode,
            CancellationToken cancellationToken = default);

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

        public WorkingTreeModel CreateWorkingTree(PhiladelphusRepositoryModel parentElement, IDataStorageModel dataStorage, bool needAutoName = true, bool withoutInfoNotifications = false);
        public TreeRootModel CreateTreeRoot(WorkingTreeModel owner, bool needAutoName = true, bool withoutInfoNotifications = false);
        public TreeNodeModel CreateTreeNode(IParentModel parentElement, bool needAutoName = true, bool withoutInfoNotifications = false);
        public TreeLeaveModel CreateTreeLeave(TreeNodeModel parentElement, bool needAutoName = true, bool withoutInfoNotifications = false);
        public ElementAttributeModel CreateElementAttribute(IAttributeOwnerModel owner, bool needAutoName = true, bool withoutInfoNotifications = false);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        public bool SoftDeleteShrubMember(IContentModel element);

        #endregion
    }
}
