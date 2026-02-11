using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Участник корня дерева сущностей
    /// </summary>
    public interface IWorkingTreeMemberModel : IShrubMemberModel, IContentModel
    {
        #region [ Properties ]

        /// <summary>
        /// Владеющее рабочее дерево
        /// </summary>
        public WorkingTreeModel OwningWorkingTree { get; }

        /// <summary>
        /// Пользовательский код
        /// Уникален в рамках дерева сущностей
        /// </summary>
        public string CustomCode { get; set; }

        #endregion

        #region [ Methods ]



        #endregion
    }
}
