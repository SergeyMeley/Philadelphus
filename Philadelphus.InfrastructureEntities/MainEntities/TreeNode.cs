using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    public class TreeNode : TreeRootMemberBase
    {
        public Guid? ParentGuid { get; set; }
        public virtual TreeRoot ParentTreeRoot { get; set; }
        public virtual TreeNode ParentTreeNode { get; set; }
        public override IMainEntity Parent
        {
            get
            {
                if (ParentTreeRoot != null)
                {
                    return ParentTreeRoot;
                }
                else
                {
                    return ParentTreeNode;
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
                    ParentTreeNode = (TreeNode)value;
                }
            }
        }
        public TreeNode()
        {

        }
    }
}
