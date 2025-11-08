using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Interfaces
{
    public interface ITreeRootMemberModel : ITreeRepositoryMemberModel
    {
        public TreeRootModel ParentRoot { get; }
    }
}
