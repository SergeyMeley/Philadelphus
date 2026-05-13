namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{

    /// <summary>
    /// Представляет объект корня рабочего дерева.
    /// </summary>
    public class TreeRoot : WorkingTreeMemberBase
    {
        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public virtual ICollection<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();

        /// <summary>
        /// Корень рабочего дерева
        /// </summary>
        public TreeRoot()
        {
            
        }
    }
}
