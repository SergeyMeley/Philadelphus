using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers
{
    public class ShrubModel : PhiladelphusRepositoryMemberBaseModel, IOwnerModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый рабочий кустарник";

        private string _alias;
        private string _customCode;
        private List<Guid> _contentTreesUuids = new List<Guid>();

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]



        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Владеющий репозиторий
        /// </summary>
        public PhiladelphusRepositoryModel OwningRepository { get; }

        /// <summary>
        /// Содержащиеся рабочие деревья
        /// </summary>
        public List<WorkingTreeModel> ContentWorkingTrees { get; }

        /// <summary>
        /// Системное рабочее дерево
        /// </summary>
        public WorkingTreeModel SystemBaseWorkingTree { get => ContentWorkingTrees.SingleOrDefault(x => x.Uuid == WorkingTreeModel.SystemBaseUuid); }

        /// <summary>
        /// Содержимое
        /// </summary>
        public ReadOnlyDictionary<Guid, IContentModel> Content 
        {
            get
            {
                var result = new Dictionary<Guid, IContentModel>();

                foreach (var item in ContentWorkingTrees)
                {
                    result.Add(item.Uuid, item);
                }

                return result.AsReadOnly();
            } 
        }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => throw new NotImplementedException(); 
        }


        /// <summary>
        /// Уникальные идентификаторы содержащихся деревьев
        /// </summary>
        public List<Guid> ContentWorkingTreesUuids 
        { 
            get
            {
                return _contentTreesUuids;
            }
            set
            {
                if (_contentTreesUuids != value)
                {
                    _contentTreesUuids = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage 
        { 
            get => OwningRepository.DataStorage;
        }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Рабочий кустарник
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="owner">Владелец</param>
        internal ShrubModel(
            Guid uuid,
            PhiladelphusRepositoryModel owner)
            : base(uuid, owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            OwningRepository = owner;

            ContentWorkingTrees = new List<WorkingTreeModel>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        public bool AddContent(IContentModel content)
        {
            if (content is WorkingTreeModel wt 
                && ContentWorkingTrees.Any(x => x.Uuid == content.Uuid) == false)
            {
                ContentWorkingTrees.Add(wt);
                ContentWorkingTreesUuids.Add(wt.Uuid);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        public bool RemoveContent(IContentModel content)
        {
            if (content is WorkingTreeModel wt
                && ContentWorkingTrees.Any(x => x.Uuid == content.Uuid))
            {
                var remItem = ContentWorkingTrees.First(x => x.Uuid == content.Uuid);
                ContentWorkingTrees.Remove(remItem);
                ContentWorkingTreesUuids.Remove(remItem.Uuid);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        public bool ClearContent()
        {
            ContentWorkingTrees.Clear();
            ContentWorkingTreesUuids.Clear();
            return true;
        }

        #endregion
    }
}
