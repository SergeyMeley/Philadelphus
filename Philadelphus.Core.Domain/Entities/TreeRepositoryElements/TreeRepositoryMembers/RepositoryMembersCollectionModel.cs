using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers
{
    public class RepositoryMembersCollectionModel : IEnumerable<IMainEntityModel>, ICollection<IMainEntityModel>
    {
        private List<TreeRootModel> _dataTreeRoots = new List<TreeRootModel>();
        public List<TreeRootModel> DataTreeRoots { get => _dataTreeRoots; private set => _dataTreeRoots = value; }

        private List<TreeNodeModel> _dataTreeNodes = new List<TreeNodeModel>();
        public List<TreeNodeModel> DataTreeNodes { get => _dataTreeNodes; private set => _dataTreeNodes = value; }

        private List<TreeLeaveModel> _dataTreeLeaves = new List<TreeLeaveModel>();
        public List<TreeLeaveModel> DataTreeLeaves { get => _dataTreeLeaves; private set => _dataTreeLeaves = value; }

        private List<ElementAttributeModel> _elementAttributes = new List<ElementAttributeModel>();
        public List<ElementAttributeModel> ElementAttributes { get => _elementAttributes; private set => _elementAttributes = value; }

        public int Count => 
            + _dataTreeRoots.Count
            + _dataTreeNodes.Count
            + _dataTreeLeaves.Count
            + _elementAttributes.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public IEnumerator<IMainEntityModel> GetEnumerator()
        {
            for (int i = 0; i < _dataTreeRoots.Count; i++)
            {
                yield return _dataTreeRoots[i];
            }
            for (int i = 0; i < _dataTreeNodes.Count; i++)
            {
                yield return _dataTreeNodes[i];
            }
            for (int i = 0; i < _dataTreeLeaves.Count; i++)
            {
                yield return _dataTreeLeaves[i];
            }
            for (int i = 0; i < _elementAttributes.Count; i++)
            {
                yield return _elementAttributes[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IMainEntityModel item)
        {
            if (item.GetType() == typeof(TreeRootModel))
            {
                _dataTreeRoots.Add((TreeRootModel)item);
            }
            else if (item.GetType() == typeof(TreeNodeModel))
            {
                _dataTreeNodes.Add((TreeNodeModel)item);
            }
            else if (item.GetType() == typeof(TreeNodeModel))
            {
                _dataTreeNodes.Add((TreeNodeModel)item);
            }
            else if (item.GetType() == typeof(ElementAttributeModel))
            {
                _elementAttributes.Add((ElementAttributeModel)item);
            }
        }

        public void Clear()
        {
            _dataTreeRoots.Clear();
            _dataTreeNodes.Clear();
            _dataTreeLeaves.Clear();
            _elementAttributes.Clear();
        }

        public bool Contains(IMainEntityModel item)
        {
            return _dataTreeRoots.Contains(item)
                || _dataTreeNodes.Contains(item)
                || _dataTreeLeaves.Contains(item)
                || _elementAttributes.Contains(item);
        }

        public void CopyTo(IMainEntityModel[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IMainEntityModel item)
        {
            if (item.GetType() == typeof(TreeRootModel))
            {
                _dataTreeRoots.Remove((TreeRootModel)item);
                return true;
            }
            else if (item.GetType() == typeof(TreeNodeModel))
            {
                _dataTreeNodes.Remove((TreeNodeModel)item);
                return true;
            }
            else if (item.GetType() == typeof(TreeNodeModel))
            {
                _dataTreeNodes.Remove((TreeNodeModel)item);
                return true;
            }
            else if (item.GetType() == typeof(ElementAttributeModel))
            {
                _elementAttributes.Remove((ElementAttributeModel)item);
                return true;
            }
            return false;
        }
    }
}
