using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.OtherEntities
{
    public class TreeRepositoryHeader
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DataStorage OwnDataStorage { get; set; }
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
    }
}
