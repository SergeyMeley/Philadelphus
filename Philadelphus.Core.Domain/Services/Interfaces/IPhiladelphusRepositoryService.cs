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
        /// <returns>Репозиторий с участниками и содержимым</returns>
        public PhiladelphusRepositoryModel GetShrubContent(
            PhiladelphusRepositoryModel repository);

        /// <summary>
        /// Получить участников и содержимое репозитория (асинхронно)
        /// </summary>
        /// <param name="repository">Репозиторий</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Репозиторий с участниками и содержимым</returns>
        public Task<PhiladelphusRepositoryModel> GetShrubContentAsync(
            PhiladelphusRepositoryModel repository,
            CancellationToken cancellationToken = default);

        #endregion

        #region [ Save ]

        /// <summary>
        /// Сохранить изменения (репозиторий)
        /// </summary>
        /// <param name="PhiladelphusRepository">Репозиторий</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(
            ref PhiladelphusRepositoryModel PhiladelphusRepository,
            SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (корни)
        /// </summary>
        /// <param name="treeRoots">Коллекция корней</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(
            IEnumerable<TreeRootModel> treeRoots, 
            SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (узлы)
        /// </summary>
        /// <param name="treeNodes">Коллекция узлов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(
            IEnumerable<TreeNodeModel> treeNodes, 
            SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (листы)
        /// </summary>
        /// <param name="treeLeaves">Коллекция листов</param>
        /// <param name="saveMode">Параметры сохранения</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveChanges(
            IEnumerable<TreeLeaveModel> treeLeaves,
            SaveMode saveMode);

        /// <summary>
        /// Сохранить изменения (атрибуты элемента)
        /// </summary>
        /// <param name="elementAttributes">Коллекция атрибутов</param>
        /// <returns>Количество сохраненных изменений</returns>
        public long SaveContentChanges(
            IEnumerable<ElementAttributeModel> elementAttributes);

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать рабочее дерево
        /// </summary>
        /// <param name="owner">Владелец</param>
        /// <param name="dataStorage">Собственное хранилище данных</param>
        /// <param name="needAutoName">Потребность в автоматической генерации наименования</param>
        /// <param name="withoutInfoNotifications">Отключить уведомления о создании</param>
        /// <returns></returns>
        public WorkingTreeModel CreateWorkingTree(
            PhiladelphusRepositoryModel owner, 
            IDataStorageModel dataStorage, 
            bool needAutoName = true, 
            bool withoutInfoNotifications = false);

        /// <summary>
        /// Создать корень
        /// </summary>
        /// <param name="owner">Владелец</param>
        /// <param name="needAutoName">Потребность в автоматической генерации наименования</param>
        /// <param name="withoutInfoNotifications">Отключить уведомления о создании</param>
        /// <returns></returns>
        public TreeRootModel CreateTreeRoot(
            WorkingTreeModel owner, 
            bool needAutoName = true, 
            bool withoutInfoNotifications = false);

        /// <summary>
        /// Создать узел
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <param name="needAutoName">Потребность в автоматической генерации наименования</param>
        /// <param name="withoutInfoNotifications">Отключить уведомления о создании</param>
        /// <returns></returns>
        public TreeNodeModel CreateTreeNode(
            IParentModel parentElement, 
            bool needAutoName = true,
            bool withoutInfoNotifications = false);

        /// <summary>
        /// Создать лист
        /// </summary>
        /// <param name="parentElement">Родитель</param>
        /// <param name="needAutoName">Потребность в автоматической генерации наименования</param>
        /// <param name="withoutInfoNotifications">Отключить уведомления о создании</param>
        /// <returns></returns>
        public TreeLeaveModel CreateTreeLeave(
            TreeNodeModel parentElement,
            bool needAutoName = true, 
            bool withoutInfoNotifications = false);

        /// <summary>
        /// Создать атрибут
        /// </summary>
        /// <param name="owner">Владелец атрибута</param>
        /// <param name="needAutoName">Потребность в автоматической генерации наименования</param>
        /// <param name="withoutInfoNotifications">Отключить уведомления о создании</param>
        /// <returns></returns>
        public ElementAttributeModel CreateElementAttribute(
            IAttributeOwnerModel owner, 
            bool needAutoName = true,
            bool withoutInfoNotifications = false);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        /// <summary>
        /// Удалить элемент рабочего кустарника (мягко)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool SoftDeleteShrubMember(
            IContentModel element);

        #endregion
    }
}
