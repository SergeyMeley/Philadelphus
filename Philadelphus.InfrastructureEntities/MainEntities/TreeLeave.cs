using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TreeLeave : TreeRootMemberBase
    {
        public virtual TreeNode ParentTreeNode { get; set; }
        public override IMainEntity Parent { get => ParentTreeNode; set => ParentTreeNode = (TreeNode)value; }
        public TreeLeave()
        {

        }

    }
}
