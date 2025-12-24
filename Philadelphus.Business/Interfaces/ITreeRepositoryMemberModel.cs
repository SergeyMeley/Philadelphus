using Philadelphus.Business.Entities.RepositoryElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Interfaces
{
    public interface ITreeRepositoryMemberModel : IMainEntityModel
    {
        public TreeRepositoryModel ParentRepository { get; }
    }
}
