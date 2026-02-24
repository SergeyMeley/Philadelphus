namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    public class PhiladelphusRepositoryHeader
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OwnDataStorageName { get; set; }
        public Guid OwnDataStorageUuid { get; set; }
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
    }
}
