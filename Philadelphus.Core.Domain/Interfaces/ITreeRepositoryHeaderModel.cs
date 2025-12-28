using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface ITreeRepositoryHeaderModel
    {
        public Guid Guid { get; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string OwnDataStorageName { get; set; }
        public Guid OwnDataStorageUuid { get; set; }
        public State State { get; }
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
    }
}
