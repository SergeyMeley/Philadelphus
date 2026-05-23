namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Представляет объект листа рабочего дерева.
    /// </summary>
    public class TreeLeave : WorkingTreeMemberBase
    {
        /// <summary>
        /// Строковое значение листа.
        /// </summary>
        public string StringValue { get; set; } = "<empty>";

        /// <summary>
        /// Родительский узел рабочего дерева.
        /// </summary>
        public Guid? ParentTreeNodeUuid { get; set; }
        
        /// <summary>
        /// Тип.
        /// </summary>
        public int SystemBaseTypeId { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeave" />.
        /// </summary>
        public TreeLeave()
        {

        }
    }
}
