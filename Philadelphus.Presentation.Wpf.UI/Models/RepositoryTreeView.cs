//using Philadelphus.Core.Domain.Entities.Enums;
//using Philadelphus.Core.Domain.Entities.RepositoryElements;
//using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace Philadelphus.Presentation.Wpf.UI.Models
//{
//    internal class RepositoryTreeViewItem
//    {
//        public Guid Uuid { get; set; }
//        public string Name { get; set; }
//        public EntityTypes EntityType { get; set; }
//        public Guid ParentUuid { get; set; }
//        public List<RepositoryTreeViewItem> Childs { get; set; }
//        public List<RepositoryTreeViewItem> GetRepositoryTreeView(TreeRepositoryModel repository)
//        {
//            var result = new List<RepositoryTreeViewItem>();
//            foreach (var root in repository.Childs)
//            {
//                var item = new RepositoryTreeViewItem();
//                item.Uuid = root.Uuid;
//                item.Name = root.Name;
//                item.EntityType = root.EntityType;
//                item.ParentUuid = root.Parent.Uuid;
//                foreach (var node in root.Childs)
//                {
//                    GetChildsFromNodes(node);
//                    Childs.Add(GetChildsFromNodes(node));
//                }
//            }
//            return result;
//        }
//        private RepositoryTreeViewItem GetChildsFromNodes(TreeNodeModel node)
//        {
//            var item = new RepositoryTreeViewItem();
//            item.Uuid = node.Uuid;
//            item.Name = node.Name;
//            item.EntityType = node.EntityType;
//            item.ParentUuid = node.Parent.Uuid;
//            if (node.Childs != null)
//            {
//                foreach (var child in node.Childs)
//                {
//                    item.Childs.Add(GetChildsFromNodes(child));
//                }
//            }
//            return item;
//        }
//    }
//}
