namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Представляет объект участника рабочего дерева.
    /// </summary>
    public abstract class WorkingTreeMemberBase : MainEntityBase
    {
        /// <summary>
        /// Владеющее рабочее дерево
        /// </summary>
        public Guid OwningWorkingTreeUuid { get; set; }
    }
}
