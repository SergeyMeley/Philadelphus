using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.WpfApplication.Models
{
    //internal class RepositoryTreeViewItem
    //{
    //    public Guid Guid { get; set; }
    //    public string Name { get; set; }
    //    public EntityTypes EntityType { get; set; }
    //    public Guid ParentGuid { get; set; }
    //    public List<RepositoryTreeViewItem> Childs { get; set; }
    //    public List<RepositoryTreeViewItem> GetRepositoryTreeView(TreeRepository repository)
    //    {
    //        var result = new List<RepositoryTreeViewItem>();
    //        foreach (var root in repository.Childs)
    //        {
    //            var item = new RepositoryTreeViewItem();
    //            item.Guid = root.Guid;
    //            item.Name = root.Name;
    //            item.EntityType = root.EntityType;
    //            item.ParentGuid = root.ParentGuid;
    //            foreach (var node in root.Childs)
    //            {
    //                GetChildsFromNodes(node);
    //                Childs.Add(GetChildsFromNodes(node));
    //            }
    //        }
    //        return result;
    //    }
    //    private RepositoryTreeViewItem GetChildsFromNodes(TreeNode node)
    //    {
    //        var item = new RepositoryTreeViewItem();
    //        item.Guid = node.Guid;
    //        item.Name = node.Name;
    //        item.EntityType = node.EntityType;
    //        item.ParentGuid = node.ParentGuid;
    //        if (node.Childs != null)
    //        {
    //            foreach (var child in node.Childs)
    //            {
    //                item.Childs.Add(GetChildsFromNodes(child));
    //            }
    //        }
    //        return item;
    //    }
    //}
}
