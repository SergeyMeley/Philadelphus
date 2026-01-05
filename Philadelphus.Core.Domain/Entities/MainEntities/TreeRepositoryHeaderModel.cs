using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Entities.MainEntities
{
    public class TreeRepositoryHeaderModel : ITreeRepositoryHeaderModel
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OwnDataStorageName { get; set; }
        public Guid OwnDataStorageUuid { get; set; }
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsHidden { get; set; }
        public State State { get; internal set; } = State.Initialized;
        internal TreeRepositoryHeaderModel() 
        { 
        }
    }
}
