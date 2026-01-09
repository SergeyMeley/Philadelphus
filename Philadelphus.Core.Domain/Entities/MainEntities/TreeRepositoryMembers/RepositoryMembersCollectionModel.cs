using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using System.Collections;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers
{
    /// <summary>
    /// Коллекция участников репозитория Чубушника
    /// </summary>
    public class RepositoryMembersCollectionModel : IEnumerable<IMainEntityModel>, ICollection<IMainEntityModel>
    {
        /// <summary>
        /// Корни репозитория Чубушника
        /// </summary>
        private List<TreeRootModel> _dataTreeRoots = new List<TreeRootModel>();

        /// <summary>
        /// Корни репозитория Чубушника
        /// </summary>
        public List<TreeRootModel> DataTreeRoots { get => _dataTreeRoots; private set => _dataTreeRoots = value; }

        /// <summary>
        /// Узлы репозитория Чубушника
        /// </summary>
        private List<TreeNodeModel> _dataTreeNodes = new List<TreeNodeModel>();

        /// <summary>
        /// Узлы репозитория Чубушника
        /// </summary>
        public List<TreeNodeModel> DataTreeNodes { get => _dataTreeNodes; private set => _dataTreeNodes = value; }

        /// <summary>
        /// Листы репозитория Чубушника
        /// </summary>
        private List<TreeLeaveModel> _dataTreeLeaves = new List<TreeLeaveModel>();

        /// <summary>
        /// Листы репозитория Чубушника
        /// </summary>
        public List<TreeLeaveModel> DataTreeLeaves { get => _dataTreeLeaves; private set => _dataTreeLeaves = value; }

        /// <summary>
        /// Атрибуты элемента
        /// </summary>
        private List<ElementAttributeModel> _elementAttributes = new List<ElementAttributeModel>();

        /// <summary>
        /// Атрибуты элемента
        /// </summary>
        public List<ElementAttributeModel> ElementAttributes { get => _elementAttributes; private set => _elementAttributes = value; }

        /// <summary>
        /// Количество элементов коллекции
        /// </summary>
        public int Count => 
            + _dataTreeRoots.Count
            + _dataTreeNodes.Count
            + _dataTreeLeaves.Count
            + _elementAttributes.Count;

        /// <summary>
        /// Коллекция толлько для чтения (не реализовано)
        /// </summary>
        public bool IsReadOnly => throw new NotImplementedException();

        /// <summary>
        /// Получить перечислитель элементов
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Получить перечислитель элементов
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Добавить элемент в коллекцию
        /// </summary>
        /// <param name="item">Элемент для добавления</param>
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

        /// <summary>
        /// Очистить коллекцию
        /// </summary>
        public void Clear()
        {
            _dataTreeRoots.Clear();
            _dataTreeNodes.Clear();
            _dataTreeLeaves.Clear();
            _elementAttributes.Clear();
        }

        /// <summary>
        /// Проверить наличие элемента в коллекции
        /// </summary>
        /// <param name="item">Искомый элемент</param>
        /// <returns></returns>
        public bool Contains(IMainEntityModel item)
        {
            return _dataTreeRoots.Contains(item)
                || _dataTreeNodes.Contains(item)
                || _dataTreeLeaves.Contains(item)
                || _elementAttributes.Contains(item);
        }

        /// <summary>
        /// Копировать коллекцию в массив (не реализовано)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CopyTo(IMainEntityModel[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Исключить элемент коллекции
        /// </summary>
        /// <param name="item">Исключаемый элемент</param>
        /// <returns></returns>
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
