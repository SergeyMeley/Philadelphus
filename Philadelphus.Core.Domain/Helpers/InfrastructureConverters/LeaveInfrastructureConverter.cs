using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class LeaveInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static TreeLeave ToDbEntity(this TreeLeaveModel businessEntity/*, TreeRepositoryService service*/)   //TODO: Заменить на сервис кеширования
        {
            if (businessEntity == null)
                return null;
            var result = (TreeLeave)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            result.ParentUuid = businessEntity.Parent.Uuid;
            result.ParentTreeRootUuid = businessEntity.ParentRoot.Uuid;
            //result.ParentTreeRoot = (TreeRoot)service.GetEntityFromCollection(businessEntity.ParentRoot.Uuid);
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
        public static List<TreeLeave> ToDbEntityCollection(this IEnumerable<TreeLeaveModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeLeave>();
            foreach (var businessEntity in businessEntityCollection)
            {
                result.Add((TreeLeave)businessEntity.ToDbEntity());
            }
            return result;
        }

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="parent">Родитель</param>
        /// <returns></returns>
        public static TreeLeaveModel ToModel(this TreeLeave dbEntity, TreeNodeModel parent)
        {
            if (dbEntity == null)
                return null;
            var result = new TreeLeaveModel(dbEntity.Uuid, parent, dbEntity);
            result = (TreeLeaveModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию сущностей БД в коллекцию доменных моделей
        /// </summary>
        /// <param name="dbEntityCollection">Коллекция сущностей БД</param>
        /// <param name="parents">Коллекция доступных ролдителей</param>
        /// <returns></returns>
        public static List<TreeLeaveModel> ToModelCollection(this IEnumerable<TreeLeave> dbEntityCollection, IEnumerable<TreeNodeModel> parents)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeLeaveModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var parent = parents.FirstOrDefault(x => x.Uuid == dbEntity.ParentUuid);
                if (parent != null)
                {
                    result.Add(dbEntity.ToModel(parent));
                }
            }
            return result;
        }
    }
}
