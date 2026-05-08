namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    public abstract class WorkingTreeMemberBase : MainEntityBase
    {
        /// <summary>
        /// Владеющее рабочее дерево
        /// </summary>
        public Guid OwningWorkingTreeUuid { get; set; }
    }
}
