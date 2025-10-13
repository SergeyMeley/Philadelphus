using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Factories;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    public static class LeaveInfrastructureConverter
    {
        public static TreeLeave ToDbEntity(this TreeLeaveModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeLeave)businessEntity.ToDbEntityGeneralProperties(new TreeLeave());
            result.ParentGuid = businessEntity.Parent.Guid;
            result.ParentTreeRootGuid = businessEntity.ParentRoot.Guid;
            result.ParentTreeRoot = (TreeRoot)TreeRepositoryService.GetEntityFromCollection(businessEntity.ParentRoot.Guid);
            return result;
        }
        public static List<TreeLeave> ToDbEntityCollection(this IEnumerable<TreeLeaveModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeLeave>();
            foreach (var businessEntity in businessEntityCollection)
            {
                result.Add(businessEntity.ToDbEntity());
            }
            return result;
        }
        public static TreeLeaveModel ToModel(this TreeLeave dbEntity, IParentModel parent)
        {
            if (dbEntity == null)
                return null;
            var result = new TreeLeaveModel(dbEntity.Guid, parent);
            result = (TreeLeaveModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }
        public static List<TreeLeaveModel> ToModelCollection(this IEnumerable<TreeLeave> dbEntityCollection, IEnumerable<IParentModel> parents)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeLeaveModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var parent = parents.FirstOrDefault(x => x.Guid == dbEntity.ParentGuid);
                if (parent != null)
                {
                    result.Add(dbEntity.ToModel(parent));
                }
            }
            return result;
        }
    }
}
