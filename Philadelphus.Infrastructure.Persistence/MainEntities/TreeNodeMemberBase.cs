using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.MainEntities
{
    public class TreeNodeMemberBase : TreeRootMemberBase
    {
        public Guid? ParentTreeNodeGuid { get; set; }
        public virtual TreeNode ParentTreeNode { get; set; }
        public virtual IMainEntity Parent
        {
            get
            {
                if (ParentTreeNode != null)
                {
                    return ParentTreeNode;
                }
                else
                {
                    return ParentTreeRoot;
                }
            }
            set
            {
                if (value.GetType() == typeof(TreeRoot))
                {
                    ParentTreeRoot = (TreeRoot)value;
                }
                else if (value.GetType() == typeof(TreeNode))
                {
                    ParentTreeRoot = ((TreeNode)value).ParentTreeRoot;
                    ParentTreeNode = (TreeNode)value;
                }
            }
        }
    }
}
