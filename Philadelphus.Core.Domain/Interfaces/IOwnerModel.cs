using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Владелец
    /// </summary>
    public interface IOwnerModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Содержимое
        /// </summary>
        public ReadOnlyDictionary<Guid, IContentModel> Content { get; }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive { get; }
    }
}
