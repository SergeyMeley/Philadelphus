using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface ITreeRepositoryHeaderModel
    {
        public Guid Uuid { get; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OwnDataStorageName { get; set; }
        public Guid OwnDataStorageUuid { get; set; }
        public State State { get; }
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
    }
}
